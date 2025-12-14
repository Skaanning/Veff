namespace Veff.Snapshot;

public class BooleanFlagSnapshot : FlagSnapshot
{
    internal BooleanFlagSnapshot(int id, string name, string description, bool isEnabled) : base(id, name, description)
    {
        IsEnabled = isEnabled;
    }
    
    public bool IsEnabled { get; }
    public bool IsDisabled => !IsEnabled;
}