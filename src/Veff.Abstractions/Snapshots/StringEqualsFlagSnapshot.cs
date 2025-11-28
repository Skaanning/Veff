namespace Veff.Abstractions.Snapshots;

public class StringEqualsFlagSnapshot(string containerName, string name, string description, string type, string[] values) 
    : FeatureFlag(containerName, name, description, type, values, null, null)
{
    public bool IsEnabled(string value)
    {
        return Values?.Any(x => x.Equals(value, StringComparison.OrdinalIgnoreCase)) == true;
    }
}