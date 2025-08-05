using System;
using Microsoft.Extensions.DependencyInjection;
using Veff.Persistence;

namespace Veff;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVeff(
        this IServiceCollection serviceCollection,
        Action<VeffSettingsBuilder> settings)
    {
        settings(new VeffSettingsBuilder(serviceCollection));
        return serviceCollection;
    }
}