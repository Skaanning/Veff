using System;
using System.Linq;

namespace Veff.Snapshot;

[Serializable]
public class StringEndsWithFlagSnapshot : StringEqualsFlagSnapshot
{
    public StringEndsWithFlagSnapshot(
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
