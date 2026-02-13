using System;
using System.Linq;

namespace Veff.Snapshot;

[Serializable]
public class StringContainsFlagSnapshot : StringEqualsFlagSnapshot
{
    public StringContainsFlagSnapshot(
        int id,
        string name,
        string description,
        string[] values) : base(id, name, description, values)
    {
    }

    public new bool EnabledFor(string value)
    {
        return Values.Any(x => x.Contains(value, StringComparison.OrdinalIgnoreCase));
    }
}