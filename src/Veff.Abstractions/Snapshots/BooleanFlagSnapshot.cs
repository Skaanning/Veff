namespace Veff.Abstractions.Snapshots;

public class BooleanFlagSnapshot(
    string containerName,
    string name,
    string type,
    string description,
    int? percent) : FeatureFlag(containerName, name, description, type, null, percent, null)
{
    public bool IsEnabled()
    {
        return Percent == 100;
    }
}