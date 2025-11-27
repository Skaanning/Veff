using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Veff.Dashboard;
using Veff.Exceptions;
using Veff.Extensions;
using Veff.Flags;
using Veff.Flags.Attributes;

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
        
        var veffFeatureFlagViewModels = all.Select(x => x.AsFlag(_veffDbConnectionFactory, []).AsDashboardViewModel()).ToArray();
        return new VeffDashboardInitViewModel(veffFeatureFlagViewModels);
    }

    public async Task SyncFeatureFlags(IEnumerable<(PropertyInfo PropInfo, string Name, string AttrName, string Type)> featureFlagNames)
    {
        var allValues = await _connection.GetAllValues();
        var flagsInCode = featureFlagNames.ToArray();
        
        var allFlags = allValues.Select(x => x.Name).ToHashSet();

        await _connection.RemoveFlagsNoLongerInCode(flagsInCode.Select(x => x.AttrName).ToArray());
        
        var flagsMissingInDb = flagsInCode.Where(x => !allFlags.Contains(x.AttrName)).ToArray();
        if (flagsMissingInDb.Length == 0) 
            return;

        var missingInDb = flagsMissingInDb.Select(x =>
        {
            var initialFlagValue = x.PropInfo.GetCustomAttributes<InitialFlagValue>().FirstOrDefault();
            return (x.AttrName, x.Type, initialFlagValue);
        }).ToArray();
        await _connection.AddFlagsMissingInDb(missingInDb);
    }

    public async Task SyncValuesFromDb(IEnumerable<IFeatureFlagContainer> veffContainers)
    {
        var allFlags =  await _connection.GetAllValues();

        var lookup = allFlags.ToLookup(x => x.GetClassName());
        var featureFlagContainers = veffContainers.ToArray();
        var propertyInfos = featureFlagContainers
            .SelectMany(z => z.GetType().GetProperties()
                .Where(x => x.PropertyType.IsAssignableTo(typeof(Flag))).Select(x => (container: z, propInfo: x)));
        var attributes = propertyInfos
            .SelectMany(x => x.propInfo.GetCustomAttributes()
                .OfType<FlagNameAttribute>()
                .Select(y => (x.container, x.propInfo, attribute: y)))
            .ToArray();

        var containerDictionary = featureFlagContainers.ToDictionary(x => x.GetType().Name);
        foreach (var mappings in attributes)
        {
            if (string.IsNullOrWhiteSpace(mappings.attribute.ContainerName)) continue;
            
            if (!containerDictionary.TryAdd(mappings.attribute.ContainerName!, mappings.container))
            {
                if (containerDictionary[mappings.attribute.ContainerName!] == mappings.container)
                    continue;
                
                throw new VeffConfigurationException($"The container name {mappings.attribute.ContainerName} is used for multiple containers. " +
                                                     $"A container name is only allowed to be used for a single container.");
            }
        }
        
        foreach (var ffClass in lookup)
        {
            if (!containerDictionary.TryGetValue(ffClass.Key, out var container)) continue;

            ffClass.ForEach(veffFlag =>
            {
                var p = container
                    .GetType()
                    .GetProperty(veffFlag.GetPropertyName())
                    ?? attributes.Where(x => x.container == container).FirstOrDefault(x=> x.attribute.Name.Equals(veffFlag.GetPropertyName())).propInfo;

                if (p is null) return;

                if (p.CanWrite)
                {
                    p.SetValue(container, veffFlag.AsFlag(_veffDbConnectionFactory, p.GetCustomAttributes()));
                }
                else
                {
                    throw new VeffConfigurationException($"Feature flag properties must have a setter, " +
                                                         $"missing setter on {veffFlag.GetClassName()}.{veffFlag.GetPropertyName()}");
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