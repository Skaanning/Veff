using System;

namespace Veff;

public interface IVeffDbConnectionFactory
{
    TimeSpan CacheExpiry { get; set; }

    IVeffDbConnection UseConnection();
}