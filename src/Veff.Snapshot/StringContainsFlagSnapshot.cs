using System;
using System.Linq;

namespace Veff.Snapshot;

public class StringContainsFlagSnapshot : StringEqualsFlagSnapshot
{
    internal StringContainsFlagSnapshot(
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