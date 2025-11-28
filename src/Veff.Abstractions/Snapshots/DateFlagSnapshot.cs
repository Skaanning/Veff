namespace Veff.Abstractions.Snapshots;

public class DateFlagSnapshot(
    string containerName,
    string name,
    string type,
    string description,
    DateTime? date) : FeatureFlag(containerName, name, description, type, null, null, date)
{
    public bool IsEnabled()
    {
        if (Date == null)
            return false;
        
        return Date >= (Date ?? DateTime.UtcNow);
    }
}