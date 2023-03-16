using System;

namespace Veff;

public interface IVeffSettingsBuilder
{
    IVeffSettingsBuilder AddCacheExpiryTime(
        TimeSpan cacheExpiry);

    IVeffSettingsBuilder AddFeatureFlagContainers(
        params IFeatureFlagContainer[] containers);
}