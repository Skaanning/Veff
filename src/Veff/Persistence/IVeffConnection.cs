using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Veff.Dashboard;
using Veff.Flags.Attributes;

namespace Veff.Persistence;

public interface IVeffConnection : IDisposable
{
    Task EnsureTablesExists();
    Task<IEnumerable<IVeffFlag>> GetAllValues();
    Task SaveUpdate(FeatureFlagUpdate featureFlagUpdate);
    Task AddFlagsMissingInDb((string AttrName, string Type, InitialFlagValue? initialValueFlag)[] flagsMissingInDb);
    HashSet<string> GetStringValueFromDb(int id, bool ignoreCase);
    int GetPercentValueFromDb(int id);
    Task RemoveFlagsNoLongerInCode(string[] allFlags);
}