using System;

namespace Veff.Snapshot;

[Serializable]
public abstract class FlagSnapshot
{
    public FlagSnapshot(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
}