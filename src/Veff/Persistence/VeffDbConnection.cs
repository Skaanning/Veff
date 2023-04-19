using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Veff.Dashboard;
using Veff.Extensions;

namespace Veff.Persistence;

internal class VeffDbConnection : IVeffDbConnection
{
    private readonly IVeffConnection _connection;
    private readonly IVeffDbConnectionFactory _veffDbConnectionFactory;

    internal VeffDbConnection(
        IVeffConnection connection,
        IVeffDbConnectionFactory veffDbConnectionFactory)
    {
        _connection = connection;
        _veffDbConnectionFactory = veffDbConnectionFactory;
    }

    public Task SaveUpdate(FeatureFlagUpdate featureFlagUpdate) => _connection.SaveUpdate(featureFlagUpdate);

    public async Task<VeffDashboardInitViewModel> GetAll()
    {
        var all = await _connection.GetAllValues();
        var veffFeatureFlagViewModels = all.Select(x => x.AsFlag(_veffDbConnectionFactory).AsDashboardViewModel()).ToArray();
        return new VeffDashboardInitViewModel(veffFeatureFlagViewModels);
    }

    public async Task SyncFeatureFlags(IEnumerable<(string Name, string Type)> featureFlagNames)
    {
        var allValues = await _connection.GetAllValues();
        
        var hashSet = allValues.Select(x => x.Name).ToHashSet();
        var flagsMissingInDb = featureFlagNames.Where(x => !hashSet.Contains(x.Name)).ToArray();

        await _connection.AddFlagsMissingInDb(flagsMissingInDb);
    }

    public async Task SyncValuesFromDb(IEnumerable<IFeatureFlagContainer> veffContainers)
    {
        var veff = await _connection.GetAllValues();

        var lookup = veff.ToLookup(x => x.GetClassName());
        var containerDictionary = veffContainers.ToDictionary(x => x.GetType().Name);

        foreach (var ffClass in lookup)
        {
            if (!containerDictionary.TryGetValue(ffClass.Key, out var container)) continue;

            ffClass.ForEach(veffFlag =>
            {
                var p = container
                    .GetType()
                    .GetProperty(veffFlag.GetPropertyName());

                if (p is null) return;

                if (p.CanWrite)
                {
                    p.SetValue(container, veffFlag.AsFlag(_veffDbConnectionFactory));
                }
                else
                {
                    var field = container
                        .GetType()
                        .GetField($"<{veffFlag.GetPropertyName()}>k__BackingField",
                            BindingFlags.Instance | BindingFlags.NonPublic);

                    field?.SetValue(container, veffFlag.AsFlag(_veffDbConnectionFactory));
                }
            });
        }
    }

    public Task EnsureTablesExists() => _connection.EnsureTablesExists();

    public HashSet<string> GetStringValueFromDb(int id) => _connection.GetStringValueFromDb(id, true);

    public int GetPercentValueFromDb(int id) => _connection.GetPercentValueFromDb(id);
    
    public void Dispose()
    {
        _connection.Dispose();
    }
}