namespace Veff.Abstractions.Snapshots;

public abstract class FlagSnapshot
{
    protected FlagSnapshot(int id, string name, string description, string[] values)
    {
        Id = id;
        Name = name;
        Description = description;
        Values = values;
    }
    
    protected FlagSnapshot(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
    
    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    protected string[]? Values { get; }
}