using System;
using System.Linq;

namespace Veff.Snapshot;

public class StringEndsWithFlagSnapshot : StringEqualsFlagSnapshot
{
    internal StringEndsWithFlagSnapshot(
        int id,
        string name,
        string description,
        string[] values) : base(id, name, description, values)
    {
    }

    public new bool EnabledFor(string value)
    {
        return Values.Any(x => x.EndsWith(value, StringComparison.OrdinalIgnoreCase));
    }
}
