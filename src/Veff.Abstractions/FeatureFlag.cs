namespace Veff.Abstractions;

public class FeatureFlag
{
    public string ContainerName { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Type { get; init; } = "";
    public string[]? Values { get; set; }
    public int? Percent { get; set; }
    public DateTime? Date { get; set; }
}