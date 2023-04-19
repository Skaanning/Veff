using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Veff.Dashboard;

namespace Veff.Persistence;

public interface IVeffDbConnection : IDisposable
{
    Task SaveUpdate(FeatureFlagUpdate featureFlagUpdate);
    Task<VeffDashboardInitViewModel> GetAll();
    Task SyncFeatureFlags(IEnumerable<(string Name, string Type)> featureFlagNames);
    Task SyncValuesFromDb(IEnumerable<IFeatureFlagContainer> veffContainers);
    Task EnsureTablesExists();
    HashSet<string> GetStringValueFromDb(int id);
    int GetPercentValueFromDb(int id);
}