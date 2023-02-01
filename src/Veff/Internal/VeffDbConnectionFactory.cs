using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Veff.Internal.Extensions;
using Veff.Internal.Requests;
using Veff.Internal.Responses;

namespace Veff.Internal
{
    public class VeffDbConnection : IVeffDbConnection
    {
        private readonly DbConnection _connection;
        private readonly VeffDbConnectionFactory _veffDbConnectionFactory;
        
        public VeffDbConnection(
            DbConnection connection,
            VeffDbConnectionFactory veffDbConnectionFactory)
        {
            _connection = connection;
            _connection.Open();
            _veffDbConnectionFactory = veffDbConnectionFactory;
        }
        
        public void SaveUpdate(FeatureFlagUpdate featureFlagUpdate)
        {
            if (_connection is SqlConnection sqlConnection)
            {
                var sqlCommand = new SqlCommand(@"
UPDATE [dbo].[Veff_FeatureFlags]
   SET [Description] = @Description
      ,[Percent] = @Percent
      ,[Strings] = @Strings
 WHERE 
    [Id] = @Id 
", sqlConnection);

                sqlCommand.Parameters.Add("@Description", SqlDbType.NVarChar).Value = featureFlagUpdate.Description;
                sqlCommand.Parameters.Add("@Percent", SqlDbType.Int).Value = featureFlagUpdate.Percent;
                var strings = featureFlagUpdate.Strings.Replace('\n', ';');
                sqlCommand.Parameters.Add("@Strings", SqlDbType.NVarChar).Value = strings;
                sqlCommand.Parameters.Add("@Id", SqlDbType.Int).Value = featureFlagUpdate.Id;

                sqlCommand.ExecuteNonQuery();
            }

        }

        public async Task<FeatureContainerViewModel> GetAll()
        {
            if (_connection is SqlConnection sqlConnection)
            {
                var sqlCommand = new SqlCommand(@"
SELECT [Id], [Name], [Description], [Percent], [Type], [Strings]
FROM Veff_FeatureFlags
", sqlConnection);
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
                        _veffDbConnectionFactory));
                }

                var array = veffDbModels.Select(x => x.AsImpl())
                    .Select(x => new FeatureFlagViewModel(x))
                    .ToArray();

                return new FeatureContainerViewModel(array);
            }

            return null!;
        }
        
        public void SyncFeatureFlags(IEnumerable<(string Name, string Type)> featureFlagNames)
        {
            if (_connection is SqlConnection sqlConnection)
            {
                using var existingFeatureFlags = new SqlCommand(@"SELECT Name FROM Veff_FeatureFlags", sqlConnection);

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

                var values = string.Join(',', flagsMissingInDb.Select((_, i) => $"(@Name{i}, @Description, @Percent, @Type{i}, @Strings)"));

                using var addFeatureFlags = new SqlCommand($"""
INSERT INTO [dbo].[Veff_FeatureFlags]
           ([Name]
           ,[Description]
           ,[Percent]
           ,[Type]
           ,[Strings])
     VALUES
           {values}
""", sqlConnection);

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
        }

        public void SyncValuesFromDb(IEnumerable<IFeatureFlagContainer> veffContainers)
        {
            if (_connection is SqlConnection sqlConnection)
            {
                using var allValuesCommand = new SqlCommand("""
SELECT [Id], [Name], [Description], [Percent], [Type], [Strings]
FROM Veff_FeatureFlags
""", sqlConnection);
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
                        sqlDataReader.GetString(5), _veffDbConnectionFactory);
                    veff.Add(flag);
                }

                var lookup = veff.ToLookup(x => x.GetClassName());
                var containerDictionary =
                    veffContainers.ToDictionary(x => x.GetType().Name);

                foreach (var ffClass in lookup)
                {
                    if (!containerDictionary.TryGetValue(ffClass.Key, out var container)) continue;

                    ffClass.ForEach(property =>
                    {
                        var p = container
                            .GetType()
                            .GetProperty(property.GetPropertyName());

                        if (p is null) return;

                        if (p.CanWrite!)
                        {
                            p.SetValue(container, property.AsImpl());
                        }
                        else
                        {
                            var field = container
                                .GetType()
                                .GetField($"<{property.GetPropertyName()}>k__BackingField",
                                    BindingFlags.Instance | BindingFlags.NonPublic);

                            field?.SetValue(container, property.AsImpl());
                        }
                    });
                }
            }

        }

        public void EnsureTablesExists()
        {
            if (_connection is SqlConnection sqlConnection)
            {
                using var command = new SqlCommand("""
EXEC sp_tables 
    @table_name = 'Veff_FeatureFlags',  
    @table_owner = 'dbo',
    @fUsePattern = 1;
""", sqlConnection);

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
""", sqlConnection);

                    createTableCmd.ExecuteNonQuery();
                }
            }
        }

        public HashSet<string> GetStringValueFromDb(int id)
        {
            if (_connection is SqlConnection sqlConnection)
            {
                using var cmd = new SqlCommand(@"
SELECT [Strings]
FROM Veff_FeatureFlags
WHERE [Id] = @Id 
", sqlConnection);
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                var percent = (string)cmd.ExecuteScalar();
                return percent.Split(";", StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.ToLower())
                    .ToHashSet();        
            }

            return new HashSet<string>();
        }

        public int GetPercentValueFromDb(int id)
        {
            if (_connection is SqlConnection sqlConnection)
            {
                using var cmd = new SqlCommand(@"
SELECT [Percent]
FROM Veff_FeatureFlags
WHERE [Id] = @Id 
", sqlConnection);
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                return (int)cmd.ExecuteScalar();
            }

            return 0;
        }

        public void Dispose()
        {
            _connection.Close();
        }
    }

    public interface IVeffDbConnection : IDisposable
    {
        void SaveUpdate(FeatureFlagUpdate featureFlagUpdate);
        Task<FeatureContainerViewModel> GetAll();
        void SyncFeatureFlags(IEnumerable<(string Name, string Type)> featureFlagNames);
        void SyncValuesFromDb(IEnumerable<IFeatureFlagContainer> veffContainers);
        void EnsureTablesExists();
        HashSet<string> GetStringValueFromDb(int id);
        int GetPercentValueFromDb(int id);
    }

    public class VeffDbConnectionFactory : IVeffDbConnectionFactory
    {
        public TimeSpan CacheExpiry { get; internal set; }
        private readonly string _connectionString;

        public VeffDbConnectionFactory(
            string connectionString,
            TimeSpan? cacheExpiry = null)
        {
            CacheExpiry = cacheExpiry ?? TimeSpan.FromSeconds(60);
            _connectionString = connectionString;
        }

        public IVeffDbConnection UseConnection()
        {
            var connection = new SqlConnection(_connectionString);

            return new VeffDbConnection(connection, this);
        }
    }
}