using System.Data;
using System.Data.SqlClient;
using Veff.Dashboard;
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
        var sqlCommand = new SqlCommand(@"
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
    
    public async Task AddFlagsMissingInDb((string Name, string Type)[] flagsMissingInDb)
    {
        var values = string.Join(',',
            flagsMissingInDb.Select((_, i) => $"(@Name{i}, @Description, @Percent, @Type{i}, @Strings)"));

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

        addFeatureFlags.Parameters.Add("@Percent", SqlDbType.Int).Value = 0;
        addFeatureFlags.Parameters.Add($"@Strings", SqlDbType.NVarChar).Value = "";
        addFeatureFlags.Parameters.Add($"@Description", SqlDbType.NVarChar).Value = "";

        for (var i = 0; i < flagsMissingInDb.Length; i++)
        {
            (var name, var type) = flagsMissingInDb[i];
            addFeatureFlags.Parameters.Add($"@Name{i}", SqlDbType.NVarChar).Value = name;
            addFeatureFlags.Parameters.Add($"@Type{i}", SqlDbType.NVarChar).Value = type;
        }

        await addFeatureFlags.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<IVeffFlag>> GetAllValues()
    {
        using var allValuesCommand = new SqlCommand("""
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
        using var command = new SqlCommand("""
EXEC sp_tables
    @table_name = 'Veff_FeatureFlags',
    @table_owner = 'dbo',
    @fUsePattern = 1;
""", _connection);

        var any = await command.ExecuteScalarAsync();

        if (any is null)
        {
            using var createTableCmd = new SqlCommand("""
CREATE TABLE Veff_FeatureFlags(
    [Id] INT PRIMARY KEY IDENTITY (1, 1),
    [Name] varchar(255),
    [Description] varchar(255),
	[Percent] int,
    [Type] varchar(255),
    [Strings] varchar(max),
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

    public void Dispose()
    {
        _connection.Dispose();
    }
}