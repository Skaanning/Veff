using Microsoft.Extensions.DependencyInjection;
using Veff.Persistence;

namespace Veff.Sqlite;

public static class VeffSettingsExtensions
{
    public static VeffSettingsBuilder AddSqlite(
        this VeffSettingsBuilder builder,
        string connectionString,
        TimeSpan? cacheExpiry)
    {
        var factory = new VeffSqliteDbConnectionFactory(connectionString)
        {
            CacheExpiry = cacheExpiry ?? TimeSpan.FromMinutes(1)
        };
        builder.ServiceCollection.AddSingleton<IVeffDbConnectionFactory>(factory);
        return builder;
    }
}