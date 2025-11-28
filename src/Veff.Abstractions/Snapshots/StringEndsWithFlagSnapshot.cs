namespace Veff.Abstractions.Snapshots;

public class StringEndsWithFlagSnapshot(int id, string name, string description, string[] values) 
    : FlagSnapshot(id, name, description, values)
{
    public bool IsEnabled(string value)
    {
        return Values?.Any(x => x.EndsWith(value, StringComparison.OrdinalIgnoreCase)) == true;
    }
}