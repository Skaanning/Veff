using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Veff.Internal.Requests;
using Veff.Internal.Responses;

namespace Veff.Internal;

public interface IVeffConnection : IDisposable
{
    void SaveUpdate(FeatureFlagUpdate featureFlagUpdate);
    Task<FeatureContainerViewModel> GetAll(IVeffDbConnectionFactory veffDbConnectionFactory);
    void SyncFeatureFlags(IEnumerable<(string Name, string Type)> featureFlagNames);
    VeffDbModel[] GetAllValues(IVeffDbConnectionFactory veffDbConnectionFactory);
    void EnsureTablesExists();
    HashSet<string> GetStringValueFromDb(int id, bool ignoreCase);
    int GetPercentValueFromDb(int id);
}