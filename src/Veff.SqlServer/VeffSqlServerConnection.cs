using System.Data;
using System.Data.SqlClient;
using Veff.Requests;
using Veff.Responses;

namespace Veff.SqlServer;

internal class VeffSqlServerConnection : IVeffConnection 
{
    private readonly SqlConnection _connection;

    public VeffSqlServerConnection(SqlConnection connection)
    {
        _connection = connection;
        _connection.Open();
    }

    public void SaveUpdate(FeatureFlagUpdate featureFlagUpdate)
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

        sqlCommand.ExecuteNonQuery();
    }

    public async Task<FeatureContainerViewModel> GetAll(IVeffDbConnectionFactory veffDbConnectionFactory)
    {
        var sqlCommand = new SqlCommand(@"
SELECT [Id], [Name], [Description], [Percent], [Type], [Strings]
FROM Veff_FeatureFlags
", _connection);
        await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();

        var veffDbModels = new List<VeffDbModel>();
        while (await sqlDataReader.ReadAsync())
        {
            veffDbModels.Add(new VeffDbModel(
                sqlDataReader.GetInt32(0),
                sqlDataReader.GetString(1),
                sqlDataReader.GetString(2),
                sqlDataReader.GetInt32(3),
                sqlDataReader.GetString(4),
                sqlDataReader.GetString(5),
                veffDbConnectionFactory));
        }

        var array = veffDbModels.Select(x => x.AsImpl())
            .Select(x => x.AsViewModel())
            .ToArray();

        return new FeatureContainerViewModel(array);
    }

    public void SyncFeatureFlags(IEnumerable<(string Name, string Type)> featureFlagNames)
    {
        using var existingFeatureFlags = new SqlCommand(@"SELECT Name FROM Veff_FeatureFlags", _connection);

        using var reader = existingFeatureFlags.ExecuteReader();
        var hashSet = new HashSet<string>();
        while (reader.Read())
            hashSet.Add(reader.GetString(0));

        reader.Close();

        var flagsMissingInDb =
            featureFlagNames.Where(x => !hashSet.Contains(x.Name)).ToArray();

        if (flagsMissingInDb.Length == 0)
        {
            return;
        }

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

        addFeatureFlags.ExecuteNonQuery();
    }

    public VeffDbModel[] GetAllValues(IVeffDbConnectionFactory veffDbConnectionFactory)
    {
        using var allValuesCommand = new SqlCommand("""
SELECT [Id], [Name], [Description], [Percent], [Type], [Strings]
FROM Veff_FeatureFlags
""", _connection);
        using var sqlDataReader = allValuesCommand.ExecuteReader();

        var veff = new List<VeffDbModel>();
        while (sqlDataReader.Read())
        {
            var flag = new VeffDbModel(
                sqlDataReader.GetInt32(0),
                sqlDataReader.GetString(1),
                sqlDataReader.GetString(2),
                sqlDataReader.GetInt32(3),
                sqlDataReader.GetString(4),
                sqlDataReader.GetString(5), veffDbConnectionFactory);
            veff.Add(flag);
        }

        return veff.ToArray();
    }

    public void EnsureTablesExists()
    {
        using var command = new SqlCommand("""
EXEC sp_tables
    @table_name = 'Veff_FeatureFlags',
    @table_owner = 'dbo',
    @fUsePattern = 1;
""", _connection);

        var any = command.ExecuteScalar();

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

            createTableCmd.ExecuteNonQuery();
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
        var percent = (string)cmd.ExecuteScalar();
        return percent.Split(";", StringSplitOptions.RemoveEmptyEntries)
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