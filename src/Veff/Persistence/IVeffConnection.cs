using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Veff.Dashboard;

namespace Veff.Persistence;

internal interface IVeffConnection : IDisposable
{
    void SaveUpdate(FeatureFlagUpdate featureFlagUpdate);
    Task<VeffDashboardInitViewModel> GetAll(IVeffDbConnectionFactory veffDbConnectionFactory);
    void SyncFeatureFlags(IEnumerable<(string Name, string Type)> featureFlagNames);
    VeffDbModel[] GetAllValues(IVeffDbConnectionFactory veffDbConnectionFactory);
    void EnsureTablesExists();
    HashSet<string> GetStringValueFromDb(int id, bool ignoreCase);
    int GetPercentValueFromDb(int id);
    
}