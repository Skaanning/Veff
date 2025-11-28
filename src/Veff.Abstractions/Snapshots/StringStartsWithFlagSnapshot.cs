namespace Veff.Abstractions.Snapshots;

public class StringStartsWithsFlagSnapshot(int id, string name, string description, string[] values) 
    : FlagSnapshot(id, name, description, values)
{
    public bool IsEnabled(string value)
    {
        return Values?.Any(x => x.StartsWith(value, StringComparison.OrdinalIgnoreCase)) == true;
    }
}