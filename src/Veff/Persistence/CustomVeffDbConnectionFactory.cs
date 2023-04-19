using System;

namespace Veff.Persistence;

internal class CustomVeffDbConnectionFactory : IVeffDbConnectionFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<IServiceProvider, IVeffConnection> _createConnection;
    public TimeSpan CacheExpiry { get; set; }

    public CustomVeffDbConnectionFactory(IServiceProvider serviceProvider, Func<IServiceProvider, IVeffConnection> createConnection, TimeSpan? cacheExpiry)
    {
        _serviceProvider = serviceProvider;
        _createConnection = createConnection;
        CacheExpiry = cacheExpiry ?? TimeSpan.FromSeconds(60);
    }
    
    public IVeffDbConnection UseConnection()
    {
        var veffConnection = _createConnection(_serviceProvider);
        return new VeffDbConnection(veffConnection, this);
    }
}