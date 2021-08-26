using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Veff.Internal.Extensions;

namespace Veff.Internal
{
    internal class UpdateInBackgroundHostedService : IHostedService, IDisposable
    {
        private readonly TimeSpan _updateTime;
        private readonly IVeffSqlConnectionFactory _sqlConnectionFactory;
        private readonly IFeatureContainer[] _featureContainers;
        private Timer _timer;

        public UpdateInBackgroundHostedService(TimeSpan updateTime, Func<Type, object> serviceLocator)
        {
            _updateTime = updateTime;
            _sqlConnectionFactory = serviceLocator(typeof(IVeffSqlConnectionFactory)) as IVeffSqlConnectionFactory;
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
            using SqlConnection sqlConnection = _sqlConnectionFactory.UseConnection();
            
            using var allValuesCommand = new SqlCommand(@"
SELECT [Id], [Name], [Description], [Percent], [Type], [Strings]
FROM Veff_FeatureFlags

", sqlConnection);
            using SqlDataReader sqlDataReader = allValuesCommand.ExecuteReader();

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
            
            ILookup<string, VeffDbModel> lookup = veff.ToLookup(x => x.GetClassName());
            Dictionary<string, IFeatureContainer> containerDictionary = _featureContainers.ToDictionary(x => x.GetType().Name);
            foreach (IGrouping<string, VeffDbModel> ffClass in lookup)
            {
                if (!containerDictionary.TryGetValue(ffClass.Key, out IFeatureContainer container)) continue;

                ffClass.ForEach(property =>
                {
                    PropertyInfo p = container
                        .GetType()
                        .GetProperty(property.GetPropertyName());

                    if (p is null) return;

                    if (p.CanWrite!)
                    {
                        p.SetValue(container, property.AsImpl());
                    }
                    else
                    {
                        FieldInfo field = container
                            .GetType()
                            .GetField($"<{property.GetPropertyName()}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

                        field?.SetValue(container, property.AsImpl());
                    }
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