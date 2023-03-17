using System;

namespace Veff;

public interface IVeffSettingsBuilder
{
    IVeffSettingsBuilder AddFeatureFlagContainersFromAssembly(params Type[] assemblyMarkers);
    IVeffSettingsBuilder AddDashboardAuthorizersFromAssembly(params Type[] assemblyMarkers);
    IVeffSettingsBuilder AddExternalApiAuthorizersFromAssembly(params Type[] assemblyMarkers);
}