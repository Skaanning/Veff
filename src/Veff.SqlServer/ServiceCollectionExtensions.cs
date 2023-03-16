using Microsoft.Extensions.DependencyInjection;

namespace Veff.SqlServer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVeff(
        this IServiceCollection serviceCollection,
        Action<VeffSqlServerBuilder> settings)
    {
        settings(new VeffSqlServerBuilder(serviceCollection));
        return serviceCollection;
    }
}