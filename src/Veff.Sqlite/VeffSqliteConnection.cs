using System.Data.SQLite;
using Veff.Dashboard;
using Veff.Persistence;

namespace Veff.Sqlite;

internal class VeffSqliteConnection : IVeffConnection 
{
    private readonly SQLiteConnection _connection;

    internal VeffSqliteConnection(SQLiteConnection connection)
    {
        _connection = connection;
        _connection.Open();
    }

    public void SaveUpdate(FeatureFlagUpdate featureFlagUpdate)
    {
        var sqlCommand = new SQLiteCommand(@"
UPDATE [Veff_FeatureFlags]
   SET [Description] = @Description
      ,[Percent] = @Percent
      ,[Strings] = @Strings
 WHERE 
    [Id] = @Id 
", _connection);

        sqlCommand.Parameters.Add(new SQLiteParameter("@Description", value: featureFlagUpdate.Description));
        sqlCommand.Parameters.Add(new SQLiteParameter("@Percent", value: featureFlagUpdate.Percent));
        var strings = featureFlagUpdate.Strings.Replace('\n', ';');
        sqlCommand.Parameters.Add(new SQLiteParameter("@Strings", value: strings));
        sqlCommand.Parameters.Add(new SQLiteParameter("@Id", value: featureFlagUpdate.Id));

        sqlCommand.ExecuteNonQuery();
    }

    public async Task<VeffDashboardInitViewModel> GetAll(IVeffDbConnectionFactory veffDbConnectionFactory)
    {
        var sqlCommand = new SQLiteCommand(@"
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

        return new VeffDashboardInitViewModel(array);
    }

    public void SyncFeatureFlags(IEnumerable<(string Name, string Type)> featureFlagNames)
    {
        using var existingFeatureFlags = new SQLiteCommand(@"SELECT Name FROM Veff_FeatureFlags", _connection);

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

        using var addFeatureFlags = new SQLiteCommand($"""
INSERT INTO [Veff_FeatureFlags]
           ([Name]
           ,[Description]
           ,[Percent]
           ,[Type]
           ,[Strings])
     VALUES
           {values}
""", _connection);

        addFeatureFlags.Parameters.Add(new SQLiteParameter("@Percent", value: 0));
        addFeatureFlags.Parameters.Add(new SQLiteParameter("@Strings", value: ""));
        addFeatureFlags.Parameters.Add(new SQLiteParameter("@Description", value: ""));

        for (var i = 0; i < flagsMissingInDb.Length; i++)
        {
            var (name, type) = flagsMissingInDb[i];
            addFeatureFlags.Parameters.Add(new SQLiteParameter($"@Name{i}", value: name));
            addFeatureFlags.Parameters.Add(new SQLiteParameter($"@Type{i}", value: type));
        }

        addFeatureFlags.ExecuteNonQuery();
    }

    public VeffDbModel[] GetAllValues(IVeffDbConnectionFactory veffDbConnectionFactory)
    {
        using var allValuesCommand = new SQLiteCommand("""
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
        using var createTableCmd = new SQLiteCommand("""
CREATE TABLE IF NOT EXISTS Veff_FeatureFlags (
            Id INTEGER PRIMARY KEY,
            Name TEXT NOT NULL,
            Description TEXT NULL,
            Percent INTEGER NOT NULL,
            Type TEXT NOT NULL,
            Strings TEXT NULL);
""", _connection);
        
        createTableCmd.ExecuteNonQuery();
    }

    public HashSet<string> GetStringValueFromDb(int id, bool ignoreCase)
    {
        var stringComparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        using var cmd = new SQLiteCommand(@"
SELECT [Strings]
FROM Veff_FeatureFlags
WHERE [Id] = @Id 
", _connection);

        cmd.Parameters.Add(new SQLiteParameter("@Id", value: id));
        var percent = (string)cmd.ExecuteScalar();
        return percent.Split(";", StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.ToLower())
            .ToHashSet(stringComparer);
    }

    public int GetPercentValueFromDb(int id)
    {
        using var cmd = new SQLiteCommand(@"
SELECT [Percent]
FROM Veff_FeatureFlags
WHERE [Id] = @Id 
", _connection);

        cmd.Parameters.Add(new SQLiteParameter("@Id", value: id));
        var executeScalar = (long)cmd.ExecuteScalar();
        return Convert.ToInt32(executeScalar);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}