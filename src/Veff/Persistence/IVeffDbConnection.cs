using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Veff.Dashboard;

namespace Veff.Persistence;

public interface IVeffDbConnection : IDisposable
{
    void SaveUpdate(FeatureFlagUpdate featureFlagUpdate);
    Task<VeffDashboardInitViewModel> GetAll();
    void SyncFeatureFlags(IEnumerable<(string Name, string Type)> featureFlagNames);
    void SyncValuesFromDb(IEnumerable<IFeatureFlagContainer> veffContainers);
    void EnsureTablesExists();
    HashSet<string> GetStringValueFromDb(int id, bool ignoreCase);
    int GetPercentValueFromDb(int id);
}