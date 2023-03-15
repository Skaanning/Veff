using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Veff.Flags;
using Veff.Internal;
using Veff.Internal.Extensions;

namespace Veff
{
    public class VeffSettingsBuilder : IUseSqlServerBuilder, IUseSqliteBuilder, IFeatureFlagContainerBuilder, IVeffCacheSettingsBuilder
    {
        private VeffDbConnectionFactory? _veffSqlConnectionFactory;
        private readonly IServiceCollection _serviceCollection;

        internal VeffSettingsBuilder(
            IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public IFeatureFlagContainerBuilder WithSqlServer(
            string connectionString)
        {
            _veffSqlConnectionFactory = new VeffDbConnectionFactory(connectionString);
            _serviceCollection.AddTransient<IVeffDbConnectionFactory>(_ => _veffSqlConnectionFactory);
            EnsureTableExists(_veffSqlConnectionFactory);
            return this;
        }

        public IFeatureFlagContainerBuilder WithSqlite(string connectionString)
        {
            _veffSqlConnectionFactory = new VeffDbConnectionFactory(connectionString);
            _serviceCollection.AddTransient<IVeffDbConnectionFactory>(_ => _veffSqlConnectionFactory);
            EnsureTableExists(_veffSqlConnectionFactory);
            return this;
        }        

        public IVeffCacheSettingsBuilder AddFeatureFlagContainers(
            params IFeatureFlagContainer[] containers)
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

        public IVeffCacheSettingsBuilder AddCacheExpiryTime(
            TimeSpan cacheExpiry)
        {
            _veffSqlConnectionFactory!.CacheExpiry = cacheExpiry;
            _serviceCollection.Replace(new ServiceDescriptor(typeof(IVeffDbConnectionFactory), _ => _veffSqlConnectionFactory, ServiceLifetime.Transient));
            return this;
        }

        private void SyncFeatureFlagsInDb(
            IEnumerable<(string Name, string Type)> featureFlagNames)
        {
            using var conn = _veffSqlConnectionFactory!.UseConnection();

            conn.SyncFeatureFlags(featureFlagNames);
        }

        private void SyncValuesFromDb(
            IEnumerable<IFeatureFlagContainer> veffContainers)
        {
            using var conn = _veffSqlConnectionFactory!.UseConnection();

            conn.SyncValuesFromDb(veffContainers);
        }

        private static void EnsureTableExists(
            IVeffDbConnectionFactory connFactory)
        {
            using var conn = connFactory.UseConnection();

            conn.EnsureTablesExists();
            
        }
    }
}