using System;

namespace Veff.Internal
{
    public interface IVeffDbConnectionFactory
    {
        TimeSpan CacheExpiry { get; set; }

        IVeffDbConnection UseConnection();
    }
}