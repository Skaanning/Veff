namespace Veff.Dashboard;

public class VeffDashboardInitViewModel
{
    public VeffDashboardInitViewModel(
        VeffFeatureFlagViewModel[] flags)
    {
        Flags = flags;
    }

    public VeffFeatureFlagViewModel[] Flags { get; set; }
}