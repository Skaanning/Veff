using System;

namespace Veff.Persistence;

public interface IVeffDbConnectionFactory
{
    TimeSpan CacheExpiry { get; set; }

    IVeffDbConnection UseConnection();
}