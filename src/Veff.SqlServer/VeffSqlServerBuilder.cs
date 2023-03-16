using Microsoft.Extensions.DependencyInjection;

namespace Veff.SqlServer;

public class VeffSqlServerBuilder : VeffSettingsBuilder
{
    protected internal VeffSqlServerBuilder(IServiceCollection serviceCollection) : base(serviceCollection)
    {
    }
    
    public IVeffSettingsBuilder WithSqlServer(
        string connectionString)
    {
        VeffSqlConnectionFactory = new VeffSqlServerDbConnectionFactory(connectionString);
        ServiceCollection.AddTransient<IVeffDbConnectionFactory>(_ => VeffSqlConnectionFactory);
        EnsureTableExists(VeffSqlConnectionFactory);
        return this;
    }
}