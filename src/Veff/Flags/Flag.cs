using Veff.Responses;

namespace Veff.Flags;

public abstract class Flag
{
    public readonly IVeffDbConnectionFactory VeffDbConnectionFactory;

    protected Flag(
        IVeffDbConnectionFactory veffDbConnectionFactory)
    {
        VeffDbConnectionFactory = veffDbConnectionFactory;
    }

    public abstract int Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }

    public abstract FeatureFlagViewModel AsViewModel();
}