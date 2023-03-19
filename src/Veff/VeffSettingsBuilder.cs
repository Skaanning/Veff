using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Veff.Dashboard;
using Veff.Extensions;
using Veff.ExternalApi;

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
    /// Uses assembly scan to register all the IFeatureFlagContainers. Uses the type markers provided to find the assemblies
    /// or if nothing is provided the entry assembly of the program.  
    /// </summary>
    /// <param name="assemblyMarkers"></param>
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
        
        // Register the flag container as its actual type
        featureFlagContainers.ForEach(x => ServiceCollection.AddSingleton(x.GetType(), x));
        // Register the flag container as one of potentially many IFeatureFlagContainer
        featureFlagContainers.ForEach(x => ServiceCollection.AddSingleton(x));

        return this;
    }

    public IVeffSettingsBuilder AddDashboardAuthorizersFromAssembly(params Type[] assemblyMarkers)
    {
        var assemblies = GetAssembliesOrDefaultToEntryAssembly(assemblyMarkers);
        var auths = assemblies
            .Select(assembly => assembly.DefinedTypes
                .Where(x => typeof(IVeffDashboardAuthorizer).IsAssignableFrom(x) 
                            && x is { IsInterface: false, IsAbstract: false }))
            .SelectMany(x => x);
        
        auths.ForEach(x => ServiceCollection.AddScoped(typeof(IVeffDashboardAuthorizer), x));

        return this;
    }

    public IVeffSettingsBuilder AddExternalApiAuthorizersFromAssembly(params Type[] assemblyMarkers)
    {
        var assemblies = GetAssembliesOrDefaultToEntryAssembly(assemblyMarkers);
        var auths = assemblies
            .Select(assembly => assembly.DefinedTypes
                .Where(x => typeof(IVeffExternalApiAuthorizer).IsAssignableFrom(x) 
                            && x is { IsInterface: false, IsAbstract: false }))
            .SelectMany(x => x);

        auths.ForEach(x => ServiceCollection.AddScoped(typeof(IVeffExternalApiAuthorizer), x));

        return this;
    }

    private static Assembly[] GetAssembliesOrDefaultToEntryAssembly(IReadOnlyCollection<Type> assemblyMarkers)
    {
        var assemblies = assemblyMarkers.SelectToArray(x => x.Assembly);
        if (assemblyMarkers.Count != 0) return assemblies;
        
        var entryAssembly = Assembly.GetEntryAssembly();
        assemblies = entryAssembly is null
            ? assemblies
            : new[] { entryAssembly };

        return assemblies;
    }
}