﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Veff.Internal.Extensions;
using Veff.Internal.Requests;
using Veff.Internal.Responses;

namespace Veff.Internal;

internal class VeffDbConnection : IVeffDbConnection
{
    private readonly IVeffConnection _connection;
    private readonly VeffSqlServerDbConnectionFactory _veffSqlServerDbConnectionFactory;

    public VeffDbConnection(
        IVeffConnection connection,
        VeffSqlServerDbConnectionFactory veffSqlServerDbConnectionFactory)
    {
        _connection = connection;
        _veffSqlServerDbConnectionFactory = veffSqlServerDbConnectionFactory;
    }

    public void SaveUpdate(FeatureFlagUpdate featureFlagUpdate) => _connection.SaveUpdate(featureFlagUpdate);

    public async Task<FeatureContainerViewModel> GetAll() => await _connection.GetAll(_veffSqlServerDbConnectionFactory);

    public void SyncFeatureFlags(IEnumerable<(string Name, string Type)> featureFlagNames) => _connection.SyncFeatureFlags(featureFlagNames);

    public void SyncValuesFromDb(IEnumerable<IFeatureFlagContainer> veffContainers)
    {
        var veff = _connection.GetAllValues(_veffSqlServerDbConnectionFactory);

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

    public void EnsureTablesExists() => _connection.EnsureTablesExists();

    public HashSet<string> GetStringValueFromDb(int id, bool ignoreCase) => _connection.GetStringValueFromDb(id, ignoreCase);

    public int GetPercentValueFromDb(int id) => _connection.GetPercentValueFromDb(id);
    
    public void Dispose()
    {
        _connection.Dispose();
    }
}