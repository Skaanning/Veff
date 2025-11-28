namespace Veff.Abstractions.Snapshots;

public class DateFlagSnapshot(
    int id,
    string name,
    string description,
    DateTime? date) : FlagSnapshot(id, name, description)
{
    public DateTime? Date { get; } = date;

    public bool IsEnabled()
    {
        if (Date == null)
            return false;
        
        return Date >= (Date ?? DateTime.UtcNow);
    }
}