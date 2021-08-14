using System;
using Microsoft.Extensions.DependencyInjection;

namespace Veff
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddVeff(this IServiceCollection serviceCollection, Action<IUseSqlServerBuilder> settings)
        {
            settings(new VeffSettingsBuilder(serviceCollection));
            return serviceCollection;
        }
    }
}