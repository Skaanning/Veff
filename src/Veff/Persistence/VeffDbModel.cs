using System;
using System.Linq;

namespace Veff.Persistence;

public interface IVeffFlag
{
    string GetClassName();
    string GetPropertyName();
    int Id { get; }
    string Name { get; }
    string Description { get; }
    int Percent { get; }
    string Type { get; }
    string[] Strings { get; }
}

internal class VeffDbModel : IVeffFlag
{
    public VeffDbModel(
        int id,
        string name,
        string description,
        int percent,
        string type,
        string strings)
    {
        Id = id;
        Name = name;
        Description = description;
        Percent = percent;
        Type = type;
        Strings = (strings ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries).ToArray();
    }

    public string GetClassName()
        => Name.Split('.', 2)[0];

    public string GetPropertyName()
        => Name.Split('.', 2)[1];

    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public int Percent { get; }
    public string Type { get; }
    public string[] Strings { get; }
}