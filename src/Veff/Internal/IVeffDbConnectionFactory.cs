using System;

namespace Veff.Internal
{
    public interface IVeffDbConnectionFactory
    {
        TimeSpan CacheExpiry { get; }

        IVeffDbConnection UseConnection();
    }
}