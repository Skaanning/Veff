using System;

namespace Veff.Snapshot;

[Serializable]
public class BooleanFlagSnapshot : FlagSnapshot
{
    public BooleanFlagSnapshot(int id, string name, string description, bool isEnabled) : base(id, name, description)
    {
        IsEnabled = isEnabled;
    }
    
    public bool IsEnabled { get; }
    public bool IsDisabled => !IsEnabled;
}