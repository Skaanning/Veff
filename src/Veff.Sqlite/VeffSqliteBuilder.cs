using Microsoft.Extensions.DependencyInjection;
using Veff.Persistence;

namespace Veff.Sqlite;

public class VeffSqliteBuilder : VeffSettingsBuilder
{
    protected internal VeffSqliteBuilder(IServiceCollection serviceCollection) : base(serviceCollection)
    {
    }
    
    public IVeffSettingsBuilder WithSqlite(
        string connectionString,
        TimeSpan? cacheExpiry)
    {
        var factory = new VeffSqliteDbConnectionFactory(connectionString)
        {
            CacheExpiry = cacheExpiry ?? TimeSpan.FromMinutes(1)
        };
        ServiceCollection.AddSingleton<IVeffDbConnectionFactory>(factory);
        return this;
    }
}