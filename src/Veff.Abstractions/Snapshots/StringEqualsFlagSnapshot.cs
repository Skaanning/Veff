namespace Veff.Abstractions.Snapshots;

public class StringEqualsFlagSnapshot(int id, string name, string description, string[] values) 
    : FlagSnapshot(id, name, description, values)
{
    public bool IsEnabled(string value)
    {
        return Values?.Any(x => x.Equals(value, StringComparison.OrdinalIgnoreCase)) == true;
    }
}