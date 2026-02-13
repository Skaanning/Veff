using System;
using System.Linq;

namespace Veff.Snapshot;

[Serializable]
public class StringEqualsFlagSnapshot : FlagSnapshot
{
    public StringEqualsFlagSnapshot(
        int id,
        string name,
        string description,
        string[] values) : base(id, name, description)
    {
        Values = values;
    }

    public string[] Values { get; }

    public bool EnabledFor(string value)
    {
        return Values.Contains(value, StringComparer.OrdinalIgnoreCase);
    }

    public bool DisabledFor(string value) => !EnabledFor(value);

    public bool EnabledForAny(params string[] values) => values.Any(EnabledFor);

    public bool EnabledForAll(params string[] values) => values.All(EnabledFor);
}
