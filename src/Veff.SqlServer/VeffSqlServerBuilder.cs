using Microsoft.Extensions.DependencyInjection;

namespace Veff.SqlServer;

public class VeffSqlServerBuilder : VeffSettingsBuilder
{
    protected internal VeffSqlServerBuilder(IServiceCollection serviceCollection) : base(serviceCollection)
    {
    }
    
    public IVeffSettingsBuilder WithSqlServer(
        string connectionString,
        TimeSpan? cacheExpiry)
    {
        ServiceCollection.AddTransient<IVeffDbConnectionFactory>(_ => NewConnectionFactory(connectionString, cacheExpiry));
        return this;
    }

    private static IVeffDbConnectionFactory NewConnectionFactory(string connectionString, TimeSpan? cacheExpiry)
    {
        return new VeffSqlServerDbConnectionFactory(connectionString)
        {
            CacheExpiry = cacheExpiry ?? TimeSpan.FromMinutes(1)
        };
    }
}