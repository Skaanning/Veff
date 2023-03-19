using Microsoft.Extensions.DependencyInjection;
using Veff.Persistence;

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
        var factory = new VeffSqlServerDbConnectionFactory(connectionString)
        {
            CacheExpiry = cacheExpiry ?? TimeSpan.FromMinutes(1)
        };
        ServiceCollection.AddSingleton<IVeffDbConnectionFactory>(factory);
        return this;
    }
}