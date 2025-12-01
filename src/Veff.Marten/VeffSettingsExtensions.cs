using Marten;
using Microsoft.Extensions.DependencyInjection;
using Veff.Persistence;

namespace Veff.Marten;

public static class VeffSettingsExtensions
{
    public static VeffSettingsBuilder AddMarten(
        this VeffSettingsBuilder builder,
        TimeSpan? cacheExpiry)
    {
        builder.ServiceCollection.AddSingleton<IVeffDbConnectionFactory>(s => new VeffMartenConnectionFactory(s, cacheExpiry));
        return builder;
    }
}
