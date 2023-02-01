namespace Veff;

public interface IFeatureFlagContainerBuilder
{
    IVeffCacheSettingsBuilder AddFeatureFlagContainers(
        params IFeatureFlagContainer[] containers);
}