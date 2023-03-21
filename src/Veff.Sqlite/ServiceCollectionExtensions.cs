using Microsoft.Extensions.DependencyInjection;

namespace Veff.Sqlite;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVeff(
        this IServiceCollection serviceCollection,
        Action<VeffSqliteBuilder> settings)
    {
        settings(new VeffSqliteBuilder(serviceCollection));
        return serviceCollection;
    }
}