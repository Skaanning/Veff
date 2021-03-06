using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Veff.Flags;
using Veff.Internal;
using Veff.Internal.Extensions;

namespace Veff
{
    public class VeffSettingsBuilder : IUseSqlServerBuilder, IFeatureFlagContainerBuilder, IBackgroundBuilder
    {
        private VeffSqlConnectionFactory? _veffSqlConnectionFactory;
        private readonly IServiceCollection _serviceCollection;

        internal VeffSettingsBuilder(
            IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public IFeatureFlagContainerBuilder WithSqlServer(
            string connectionString)
        {
            _veffSqlConnectionFactory = new VeffSqlConnectionFactory(connectionString);
            _serviceCollection.AddTransient<IVeffSqlConnectionFactory>(_ => _veffSqlConnectionFactory);
            EnsureTableExists(_veffSqlConnectionFactory);
            return this;
        }

        public IBackgroundBuilder AddFeatureFlagContainers(
            params IFeatureContainer[] containers)
        {
            var featureFlagNames = new List<(string, string)>();
            foreach (var veffContainer in containers)
            {
                var type = veffContainer.GetType();
                var targetType = typeof(Flag);

                type.GetProperties()
                    .Where(x => x.PropertyType.IsAssignableTo(targetType))
                    .Select(x => ($"{type.Name}.{x.Name}", x.PropertyType.ToString()))
                    .ForEach(x => featureFlagNames.Add(x));
            }

            SyncFeatureFlagsInDb(featureFlagNames);
            SyncValuesFromDb(containers);

            containers.ForEach(x => _serviceCollection.AddSingleton(x.GetType(), x));
            containers.ForEach(x => _serviceCollection.AddSingleton(x));

            return this;
        }

        public IBackgroundBuilder AddCacheExpiryTime(
            TimeSpan cacheExpiry)
        {
            _veffSqlConnectionFactory!.CacheExpiry = cacheExpiry;
            _serviceCollection.Replace(new ServiceDescriptor(typeof(IVeffSqlConnectionFactory), _ => _veffSqlConnectionFactory, ServiceLifetime.Transient));
            return this;
        }

        private void SyncFeatureFlagsInDb(
            IEnumerable<(string Name, string Type)> featureFlagNames)
        {
            using var conn = _veffSqlConnectionFactory!.UseConnection();

            using var existingFeatureFlags = new SqlCommand(@"SELECT Name FROM Veff_FeatureFlags", conn);

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

            using var addFeatureFlags = new SqlCommand(@$"
INSERT INTO [dbo].[Veff_FeatureFlags]
           ([Name]
           ,[Description]
           ,[Percent]
           ,[Type]
           ,[Strings])
     VALUES
           {values}", conn);

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

        private void SyncValuesFromDb(
            IFeatureContainer[] veffContainers)
        {
            using var conn = _veffSqlConnectionFactory!.UseConnection();

            using var allValuesCommand = new SqlCommand(@"
SELECT [Id], [Name], [Description], [Percent], [Type], [Strings]
FROM Veff_FeatureFlags

", conn);
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
                    sqlDataReader.GetString(5), _veffSqlConnectionFactory);
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

        private static void EnsureTableExists(
            IVeffSqlConnectionFactory connFactory)
        {
            using var conn = connFactory.UseConnection();

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
        }
    }

    public interface IUseSqlServerBuilder
    {
        IFeatureFlagContainerBuilder WithSqlServer(
            string connectionString);
    }

    public interface IFeatureFlagContainerBuilder
    {
        IBackgroundBuilder AddFeatureFlagContainers(
            params IFeatureContainer[] containers);
    }

    public interface IBackgroundBuilder
    {
        IBackgroundBuilder AddCacheExpiryTime(
            TimeSpan cacheExpiry);
    }
}