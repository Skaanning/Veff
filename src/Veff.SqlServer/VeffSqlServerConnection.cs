using System.Data;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Veff.Dashboard;
using Veff.Exceptions;
using Veff.Flags.Attributes;
using Veff.Persistence;

namespace Veff.SqlServer;

internal class VeffSqlServerConnection : IVeffConnection
{
    private readonly SqlConnection _connection;

    internal VeffSqlServerConnection(SqlConnection connection)
    {
        _connection = connection;
        _connection.Open();
    }

    public async Task SaveUpdate(FeatureFlagUpdate featureFlagUpdate)
    {
        await using var sqlCommand = new SqlCommand(@"
UPDATE [dbo].[Veff_FeatureFlags]
   SET [Description] = @Description
      ,[Percent] = @Percent
      ,[Strings] = @Strings
 WHERE 
    [Id] = @Id 
", _connection);

        sqlCommand.Parameters.Add("@Description", SqlDbType.NVarChar).Value = featureFlagUpdate.Description;
        sqlCommand.Parameters.Add("@Percent", SqlDbType.Int).Value = featureFlagUpdate.Percent;
        var strings = featureFlagUpdate.Strings.Replace('\n', ';');
        sqlCommand.Parameters.Add("@Strings", SqlDbType.NVarChar).Value = strings;
        sqlCommand.Parameters.Add("@Id", SqlDbType.Int).Value = featureFlagUpdate.Id;

        await sqlCommand.ExecuteNonQueryAsync();
    }

    public async Task AddFlagsMissingInDb(
        (string AttrName, string Type, InitialFlagValue? initialValueFlag)[] flagsMissingInDb)
    {
        var values = string.Join(',',
            flagsMissingInDb.Select((_, i) => $"(@Name{i}, @Description, @Percent{i}, @Type{i}, @Strings{i})"));

        if (values.Length == 0)
            return;

        using var addFeatureFlags = new SqlCommand($"""
                                                    INSERT INTO [dbo].[Veff_FeatureFlags]
                                                               ([Name]
                                                               ,[Description]
                                                               ,[Percent]
                                                               ,[Type]
                                                               ,[Strings])
                                                         VALUES
                                                               {values}
                                                    """, _connection);

        addFeatureFlags.Parameters.Add($"@Description", SqlDbType.NVarChar).Value = "";

        for (var i = 0; i < flagsMissingInDb.Length; i++)
        {
            var (name, type, attribute) = flagsMissingInDb[i];

            if (attribute is not null && !attribute.IsValidFor(type))
                throw new VeffConfigurationException($"The InitialFlagValue attribute is not valid for the flag {name} of type {type}");
            
            addFeatureFlags.Parameters.Add($"@Percent{i}", SqlDbType.Int).Value = attribute?.Percentage ?? 0;
            addFeatureFlags.Parameters.Add($"@Strings{i}", SqlDbType.NVarChar).Value = attribute?.Value ?? "";
            addFeatureFlags.Parameters.Add($"@Name{i}", SqlDbType.NVarChar).Value = name;
            addFeatureFlags.Parameters.Add($"@Type{i}", SqlDbType.NVarChar).Value = type;
        }

        await addFeatureFlags.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<IVeffFlag>> GetAllValues()
    {
        await using var allValuesCommand = new SqlCommand("""
                                                          SELECT [Id], [Name], [Description], [Percent], [Type], [Strings]
                                                          FROM Veff_FeatureFlags
                                                          """, _connection);
        await using var sqlDataReader = await allValuesCommand.ExecuteReaderAsync();

        var veff = new List<IVeffFlag>();
        while (await sqlDataReader.ReadAsync())
        {
            var flag = new VeffDbModel(
                sqlDataReader.GetInt32(0),
                sqlDataReader.GetString(1),
                sqlDataReader.GetString(2),
                sqlDataReader.GetInt32(3),
                sqlDataReader.GetString(4),
                sqlDataReader.GetString(5));
            veff.Add(flag);
        }

        return veff.ToArray();
    }

    public async Task EnsureTablesExists()
    {
        await using var command = new SqlCommand("""
                                                 EXEC sp_tables
                                                     @table_name = 'Veff_FeatureFlags',
                                                     @table_owner = 'dbo',
                                                     @fUsePattern = 1;
                                                 """, _connection);

        var any = await command.ExecuteScalarAsync();

        if (any is null)
        {
            await using var createTableCmd = new SqlCommand("""
                CREATE TABLE Veff_FeatureFlags(
                    [Id] INT PRIMARY KEY IDENTITY (1, 1),
                    [Name] varchar(255),
                    [Description] varchar(255),
                    [Percent] int,
                    [Type] varchar(255),
                    [Strings] varchar(1024)
                    )
                """, _connection);

            await createTableCmd.ExecuteNonQueryAsync();
        }
    }

    public HashSet<string> GetStringValueFromDb(int id, bool ignoreCase)
    {
        var stringComparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        using var cmd = new SqlCommand(@"
SELECT [Strings]
FROM Veff_FeatureFlags
WHERE [Id] = @Id 
", _connection);

        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        var strings = (string)cmd.ExecuteScalar();
        return strings.Split(";", StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.ToLower())
            .ToHashSet(stringComparer);
    }

    public int GetPercentValueFromDb(int id)
    {
        using var cmd = new SqlCommand(@"
SELECT [Percent]
FROM Veff_FeatureFlags
WHERE [Id] = @Id 
", _connection);

        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        return (int)cmd.ExecuteScalar();
    }

    public async Task RemoveFlagsNoLongerInCode(string[] allFlags)
    {
        var paramNames = allFlags.Select((_, i) => $"@Name{i}").ToArray();
        var sql = $"DELETE FROM Veff_FeatureFlags WHERE [Name] NOT IN ({string.Join(", ", paramNames)})";

        await using var cmd = new SqlCommand(sql, _connection);
        var i = 0;
        foreach (var name in allFlags)
        {
            cmd.Parameters.Add(paramNames[i++], SqlDbType.NVarChar).Value = name;
        } 
        
        await cmd.ExecuteNonQueryAsync();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}