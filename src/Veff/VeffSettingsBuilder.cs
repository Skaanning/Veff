using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Veff.Extensions;

namespace Veff;

public class VeffSettingsBuilder : IVeffSettingsBuilder
{
    protected readonly IServiceCollection ServiceCollection;

    protected VeffSettingsBuilder(
        IServiceCollection serviceCollection)
    {
        ServiceCollection = serviceCollection;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IVeffSettingsBuilder AddFeatureFlagContainersFromAssembly(params Type[] assemblyMarkers)
    {
        var assemblies = GetAssembliesOrDefaultToEntryAssembly(assemblyMarkers);

        var containers = assemblies
            .Select(assembly => assembly.DefinedTypes
                .Where(x => typeof(IFeatureFlagContainer).IsAssignableFrom(x) 
                            && x is { IsInterface: false, IsAbstract: false }));

        var featureFlagContainers = containers
            .SelectMany(x => x)
            .Select(Activator.CreateInstance)
            .Cast<IFeatureFlagContainer>()
            .ToArray();
        
        return AddFeatureFlagContainers(featureFlagContainers);
    }

    public IVeffSettingsBuilder AddDashboardAuthorizersFromAssembly(params Type[] assemblyMarkers)
    {
        var assemblies = GetAssembliesOrDefaultToEntryAssembly(assemblyMarkers);
        var auths = assemblies
            .Select(assembly => assembly.DefinedTypes
                .Where(x => typeof(IVeffDashboardAuthorizer).IsAssignableFrom(x) 
                            && x is { IsInterface: false, IsAbstract: false }));
        
        foreach (var typeInfos in auths)
        {
            
        }

        return this;
    }

    public IVeffSettingsBuilder AddExternalApiAuthorizersFromAssembly(params Type[] assemblyMarkers)
    {
        var assemblies = GetAssembliesOrDefaultToEntryAssembly(assemblyMarkers);
        var auths = assemblies
            .Select(assembly => assembly.DefinedTypes
                .Where(x => typeof(IVeffExternalApiAuthorizer).IsAssignableFrom(x) 
                            && x is { IsInterface: false, IsAbstract: false }));

        return this;
    }

    private IVeffSettingsBuilder AddFeatureFlagContainers(
        params IFeatureFlagContainer[] containers)
    {
        containers.ForEach(x => ServiceCollection.AddSingleton(x.GetType(), x));
        containers.ForEach(x => ServiceCollection.AddSingleton(x));

        return this;
    }
    
    private static Assembly[] GetAssembliesOrDefaultToEntryAssembly(Type[] assemblyMarkers)
    {
        var assemblies = assemblyMarkers.SelectToArray(x => x.Assembly);
        if (assemblyMarkers.Length != 0) return assemblies;
        
        var entryAssembly = Assembly.GetEntryAssembly();
        assemblies = entryAssembly is null
            ? assemblies
            : new[] { entryAssembly };

        return assemblies;
    }
}