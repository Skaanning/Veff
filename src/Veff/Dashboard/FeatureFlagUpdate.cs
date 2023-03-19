namespace Veff.Dashboard;

public class FeatureFlagUpdate
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public string Strings { get; set; } = "";
    public int Percent { get; set; }
}