namespace Veff.Abstractions;

public class FeatureFlag
{
    public FeatureFlag() { }
    public FeatureFlag(string containerName, string name, string description, string type, string[]? values, int? percent, DateTime? date)
    {
        ContainerName = containerName;
        Name = name;
        Description = description;
        Type = type;
        Values = values;
        Percent = percent;
        Date = date;
    }

    public string ContainerName { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Type { get; init; } = "";
    public string[]? Values { get; set; }
    public int? Percent { get; set; }
    public DateTime? Date { get; set; }
}