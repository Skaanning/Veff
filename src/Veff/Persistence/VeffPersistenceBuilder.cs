using System;
using Microsoft.Extensions.DependencyInjection;

namespace Veff.Persistence;

public class VeffPersistenceBuilder : VeffSettingsBuilder
{
    public VeffPersistenceBuilder(IServiceCollection serviceCollection) : base(serviceCollection)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="createConnection"></param>
    /// <param name="cacheExpiry">defaults to 60 seconds</param>
    /// <returns></returns>
    public IVeffSettingsBuilder AddPersistence(
        Func<IServiceProvider, IVeffConnection> createConnection,
        TimeSpan? cacheExpiry = null)
    {
        ServiceCollection.AddSingleton<IVeffDbConnectionFactory>(ctx => new CustomVeffDbConnectionFactory(ctx, createConnection, cacheExpiry));
        return this;
    }
}