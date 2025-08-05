using Microsoft.Extensions.DependencyInjection;
using Veff.Persistence;

namespace Veff.SqlServer;

public static class VeffSettingsExtensions
{
    public static VeffSettingsBuilder AddSqlServer(
        this VeffSettingsBuilder builder,
        string connectionString,
        TimeSpan? cacheExpiry)
    {
        var factory = new VeffSqlServerDbConnectionFactory(connectionString)
        {
            CacheExpiry = cacheExpiry ?? TimeSpan.FromMinutes(1)
        };
        builder.ServiceCollection.AddSingleton<IVeffDbConnectionFactory>(factory);
        return builder;
    }
}