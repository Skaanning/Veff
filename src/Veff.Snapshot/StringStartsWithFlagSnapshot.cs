using System;
using System.Linq;

namespace Veff.Snapshot;

public class StringStartsWithFlagSnapshot : StringEqualsFlagSnapshot
{
    internal StringStartsWithFlagSnapshot(
        int id,
        string name,
        string description,
        string[] values) : base(id, name, description, values)
    {
    }

    public new bool EnabledFor(string value)
    {
        return Values.Any(x => x.StartsWith(value, StringComparison.OrdinalIgnoreCase));
    }
}
