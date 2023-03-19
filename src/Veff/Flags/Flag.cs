using Veff.Dashboard;
using Veff.Persistence;

namespace Veff.Flags;

public abstract class Flag
{
    internal readonly IVeffDbConnectionFactory VeffDbConnectionFactory;
    internal Flag(
        IVeffDbConnectionFactory veffDbConnectionFactory)
    {
        VeffDbConnectionFactory = veffDbConnectionFactory;
    }

    public abstract int Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }

    internal abstract VeffFeatureFlagViewModel AsViewModel();
}