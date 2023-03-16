using Microsoft.Extensions.DependencyInjection;
using Veff.Internal;

namespace Veff.SqlServer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVeff(
        this IServiceCollection serviceCollection,
        Action<IUseSqlServerBuilder> settings)
    {
        settings(new VeffSqlServerBuilder(serviceCollection));
        return serviceCollection;
    }
}

public class VeffSqlServerBuilder : VeffSettingsBuilder, IUseSqlServerBuilder
{
    protected internal VeffSqlServerBuilder(IServiceCollection serviceCollection) : base(serviceCollection)
    {
    }
    
    public IFeatureFlagContainerBuilder WithSqlServer(
        string connectionString)
    {
        VeffSqlConnectionFactory = new VeffSqlServerDbConnectionFactory(connectionString);
        ServiceCollection.AddTransient<IVeffDbConnectionFactory>(_ => VeffSqlConnectionFactory);
        EnsureTableExists(VeffSqlConnectionFactory);
        return this;
    }
}