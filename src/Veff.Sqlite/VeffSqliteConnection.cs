using System.Data.SQLite;
using Veff.Dashboard;
using Veff.Exceptions;
using Veff.Flags.Attributes;
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

    public async Task SaveUpdate(FeatureFlagUpdate featureFlagUpdate)
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

        await sqlCommand.ExecuteNonQueryAsync();
    }

    public async Task AddFlagsMissingInDb((string AttrName, string Type, InitialFlagValue? initialValueFlag)[] flagsMissingInDb)
    {
        var values = string.Join(',', flagsMissingInDb.Select((_, i) => $"(@Name{i}, @Description, @Percent{i}, @Type{i}, @Strings{i})"));
        if (values.Length == 0)
            return;
            
        await using var addFeatureFlags = new SQLiteCommand($"""
INSERT INTO [Veff_FeatureFlags]
           ([Name]
           ,[Description]
           ,[Percent]
           ,[Type]
           ,[Strings])
     VALUES
           {values}
""", _connection);

        addFeatureFlags.Parameters.Add(new SQLiteParameter("@Description", value: ""));

        for (var i = 0; i < flagsMissingInDb.Length; i++)
        {
            var (name, type, attribute) = flagsMissingInDb[i];

            if (attribute is not null && !attribute.IsValidFor(type))
                throw new VeffConfigurationException($"The InitialFlagValue attribute is not valid for the flag {name} of type {type}");
            
            addFeatureFlags.Parameters.Add(new SQLiteParameter($"@Percent{i}", value: attribute?.Percentage ?? 0));
            addFeatureFlags.Parameters.Add(new SQLiteParameter($"@Strings{i}", value: attribute?.Value ?? ""));
            addFeatureFlags.Parameters.Add(new SQLiteParameter($"@Name{i}", value: name));
            addFeatureFlags.Parameters.Add(new SQLiteParameter($"@Type{i}", value: type));
        }

        await addFeatureFlags.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<IVeffFlag>> GetAllValues()
    {
        using var allValuesCommand = new SQLiteCommand("""
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
        await using var createTableCmd = new SQLiteCommand("""
CREATE TABLE IF NOT EXISTS Veff_FeatureFlags (
            Id INTEGER PRIMARY KEY,
            Name TEXT NOT NULL,
            Description TEXT NULL,
            Percent INTEGER NOT NULL,
            Type TEXT NOT NULL,
            Strings TEXT NULL);
""", _connection);

        await createTableCmd.ExecuteNonQueryAsync();
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
        var strings = (string)cmd.ExecuteScalar();
        return strings.Split(";", StringSplitOptions.RemoveEmptyEntries)
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

    public async Task RemoveFlagsNoLongerInCode(string[] allFlags)
    {
        var paramNames = allFlags.Select((_, i) => $"@Name{i}").ToArray();
        var sql = $"DELETE FROM Veff_FeatureFlags WHERE [Name] NOT IN ({string.Join(", ", paramNames)})";

        await using var cmd = new SQLiteCommand(sql, _connection);
        var i = 0;
        foreach (var name in allFlags)
        {
            cmd.Parameters.Add(new SQLiteParameter(paramNames[i++], value: name));
        }

        await cmd.ExecuteNonQueryAsync();    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}