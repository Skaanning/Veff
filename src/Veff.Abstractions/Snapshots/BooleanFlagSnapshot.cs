namespace Veff.Abstractions.Snapshots;

public class BooleanFlagSnapshot(
    int id,
    string name,
    string description,
    bool isEnabled) : FlagSnapshot(id, name, description)
{
    public bool IsEnabled()
    {
        return isEnabled;
    }
}