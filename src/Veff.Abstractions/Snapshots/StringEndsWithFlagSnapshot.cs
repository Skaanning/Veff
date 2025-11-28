namespace Veff.Abstractions.Snapshots;

public class StringEndsWithFlagSnapshot(string containerName, string name, string description, string type, string[] values) 
    : FeatureFlag(containerName, name, description, type, values, null, null)
{
    public bool IsEnabled(string value)
    {
        return Values?.Any(x => x.EndsWith(value, StringComparison.OrdinalIgnoreCase)) == true;
    }
}