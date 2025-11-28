namespace Veff.Abstractions.Snapshots;

public class StringStartsWithFlagSnapshot(string containerName, string name, string description, string type, string[] values) 
    : FeatureFlag(containerName, name, description, type, values, null, null)
{
    public bool IsEnabled(string value)
    {
        return Values?.Any(x => x.StartsWith(value, StringComparison.OrdinalIgnoreCase)) == true;
    }
}