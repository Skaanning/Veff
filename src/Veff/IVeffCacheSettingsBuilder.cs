using System;

namespace Veff;

public interface IVeffCacheSettingsBuilder
{
    IVeffCacheSettingsBuilder AddCacheExpiryTime(
        TimeSpan cacheExpiry);
}