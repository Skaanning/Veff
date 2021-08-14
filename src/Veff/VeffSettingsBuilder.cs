using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Veff.Extensions;
using Veff.Flags;

namespace Veff
{
    public class VeffSettingsBuilder : IUseSqlServerBuilder, IFeatureFlagContainerBuilder, IBackgroundBuilder
    {
        private SqlConnection _sqlServerConnection;
        private readonly IServiceCollection _serviceCollection;
        private IFeatureContainer[] _containers;

        internal VeffSettingsBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }
        
        public IFeatureFlagContainerBuilder UseSqlServer(string connectionString)
        {
            _sqlServerConnection = new SqlConnection(connectionString);
            EnsureTableExists(_sqlServerConnection);          
            return this;
        }
        
        public IBackgroundBuilder AddFeatureFlagContainers(params IFeatureContainer[] containers)
        {
            _containers = containers;
            var featureFlagNames = new List<(string,string)>();
            foreach (var veffContainer in containers)
            {
                var type = veffContainer.GetType();
                var targetType = typeof(Flag);

                type.GetProperties()
                    .Where(x => x.PropertyType.IsAssignableTo(targetType))
                    .Select(x => ($"{type.Name}.{x.Name}", x.PropertyType.ToString()))
                    .ForEach(x => featureFlagNames.Add((x)));
            }

            SyncFeatureFlagsInDb(featureFlagNames);
            SyncValuesFromDb(containers);
            
            containers.ForEach(x => _serviceCollection.AddSingleton(x.GetType(), x));
            containers.ForEach(x => _serviceCollection.AddSingleton<IFeatureContainer>(x));
            _serviceCollection.AddSingleton(_sqlServerConnection);
            
            return this;
        }
        
        public IBackgroundBuilder UpdateInBackground(TimeSpan updateTime)
        {
            _serviceCollection.AddHostedService(x => new TimedHostedService(updateTime, x.GetService));
            return this;
        }

        private void SyncFeatureFlagsInDb(IEnumerable<(string Name, string Type)> featureFlagNames)
        {
            _sqlServerConnection.Open();

            using var existingFeatureFlags = new SqlCommand(@"SELECT Name FROM Veff_FeatureFlags", _sqlServerConnection);

            using var reader = existingFeatureFlags.ExecuteReader();
            var hashSet = new HashSet<string>();
            while (reader.Read())
                hashSet.Add(reader.GetString(0));
            
            reader.Close();
            
            var flagsMissingInDb = featureFlagNames.Where(x => !hashSet.Contains(x.Name)).ToArray();

            if (flagsMissingInDb.Length == 0)
            {
                _sqlServerConnection.Close();
                return;
            }

            var i = 0;
            var values = string.Join(',', flagsMissingInDb.Select(x =>
            {
                i++;
                return $"(@Name{i}, @Description, @Percent, @Type{i}, @Strings)";
            }));
            
            using var addFeatureFlags = new SqlCommand(@$"
INSERT INTO [dbo].[Veff_FeatureFlags]
           ([Name]
           ,[Description]
           ,[Percent]
           ,[Type]
           ,[Strings])
     VALUES
           {values}" , _sqlServerConnection);

            addFeatureFlags.Parameters.Add("@Percent", SqlDbType.Int).Value = 0;
            addFeatureFlags.Parameters.Add($"@Strings", SqlDbType.NVarChar).Value = "";
            addFeatureFlags.Parameters.Add($"@Description", SqlDbType.NVarChar).Value = "";

            i = 0;
            flagsMissingInDb.ForEach(x =>
            {
                i++;
                addFeatureFlags.Parameters.Add($"@Name{i}", SqlDbType.NVarChar).Value = x.Name;
                addFeatureFlags.Parameters.Add($"@Type{i}", SqlDbType.NVarChar).Value = x.Type;
            });
            
            addFeatureFlags.ExecuteNonQuery();
            _sqlServerConnection.Close();
        }
        
        private void SyncValuesFromDb(IFeatureContainer[] veffContainers)
        {
            _sqlServerConnection.Open();

            using var allValuesCommand = new SqlCommand(@"
SELECT [Id], [Name], [Description], [Percent], [Type], [Strings]
FROM Veff_FeatureFlags

", _sqlServerConnection);
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
                    sqlDataReader.GetString(5));
                veff.Add(flag);   
            }
            _sqlServerConnection.Close();

            var lookup = veff.ToLookup(x => x.GetClassName());
            var containerDictionary = veffContainers.ToDictionary(x => x.GetType().Name);
            foreach (var ffClass in lookup)
            {
                if (!containerDictionary.TryGetValue(ffClass.Key, out var container)) continue;

                ffClass.ForEach(property =>
                {
                    var p = container
                        .GetType()
                        .GetProperty(property.GetPropertyName());
                    p?.SetValue(container, property.AsImpl());
                });
            }
        }

        private static void EnsureTableExists(SqlConnection conn)
        {
            conn.Open();
            
            using var command = new SqlCommand(@"
EXEC sp_tables 
    @table_name = 'Veff_FeatureFlags',  
    @table_owner = 'dbo',
    @fUsePattern = 1;
", conn);

            var any = command.ExecuteScalar();

            if (any is null)
            {
                using var createTableCmd = new SqlCommand(@"
CREATE TABLE Veff_FeatureFlags(
    [Id] INT PRIMARY KEY IDENTITY (1, 1),
    [Name] varchar(255),
    [Description] varchar(255),
	[Percent] int,
    [Type] varchar(255),
    [Strings] varchar(max),
)
", conn);

                createTableCmd.ExecuteNonQuery();
            }
            
            conn.Close();
        }
    }

    public interface IUseSqlServerBuilder
    {
        IFeatureFlagContainerBuilder UseSqlServer(string connectionString);
    }
    
    public interface IFeatureFlagContainerBuilder
    {
        IBackgroundBuilder AddFeatureFlagContainers(params IFeatureContainer[] containers);
    }

    public interface IBackgroundBuilder
    {
        IBackgroundBuilder UpdateInBackground(TimeSpan updateTime);
    }

    public class TimedHostedService : IHostedService, IDisposable
    {
        private readonly TimeSpan _updateTime;
        private readonly SqlConnection _sqlConnection;
        private readonly IFeatureContainer[] _featureContainers;
        private Timer _timer;

        public TimedHostedService(TimeSpan updateTime, Func<Type, object> serviceLocator)
        {
            _updateTime = updateTime;
            //todo fix
            _sqlConnection = serviceLocator(typeof(SqlConnection)) as SqlConnection;
            _featureContainers = ((serviceLocator(typeof(IEnumerable<IFeatureContainer>)) as IEnumerable<IFeatureContainer>) ?? Array.Empty<IFeatureContainer>()).ToArray();
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, _updateTime);
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            //todo this and SyncValuesFromDb are almost identical
            _sqlConnection.Open();
            
            using var allValuesCommand = new SqlCommand(@"
SELECT [Id], [Name], [Description], [Percent], [Type], [Strings]
FROM Veff_FeatureFlags

", _sqlConnection);
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
                    sqlDataReader.GetString(5));
                veff.Add(flag);   
            }
            _sqlConnection.Close();
            
            var lookup = veff.ToLookup(x => x.GetClassName());
            var containerDictionary = _featureContainers.ToDictionary(x => x.GetType().Name);
            foreach (var ffClass in lookup)
            {
                if (!containerDictionary.TryGetValue(ffClass.Key, out var container)) continue;

                ffClass.ForEach(property =>
                {
                    var p = container
                        .GetType()
                        .GetProperty(property.GetPropertyName());
                    p?.SetValue(container, property.AsImpl());
                });
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}