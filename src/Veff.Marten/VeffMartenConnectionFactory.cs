using Marten;
using Microsoft.Extensions.DependencyInjection;
using Veff.Persistence;

namespace Veff.Marten;

internal class VeffMartenConnectionFactory : IVeffDbConnectionFactory
{
    private readonly IServiceProvider _serviceProvider;
    public TimeSpan CacheExpiry { get; set; }

    internal VeffMartenConnectionFactory(
        IServiceProvider serviceProvider,
        TimeSpan? cacheExpiry = null)
    {
        _serviceProvider = serviceProvider;
        CacheExpiry = cacheExpiry ?? TimeSpan.FromSeconds(60);
    }

    public IVeffDbConnection UseConnection()
    {
        var documentStore = _serviceProvider.GetRequiredService<IDocumentStore>();
        return new VeffDbConnection(new VeffMartenConnection(documentStore), this);
    }
}