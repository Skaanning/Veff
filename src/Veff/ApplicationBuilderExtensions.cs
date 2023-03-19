using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Veff.Extensions;
using Veff.Flags;
using Veff.Persistence;

namespace Veff;

public static class ApplicationBuilderExtensions
{ 
    /// <summary>
    /// Syncs the database table with the feature flag containers, and setups the flag containers to be ready to go. 
    /// </summary>
    /// <param name="appBuilder"></param>
    /// <param name="configBuilder">Use the config builder to additional veff components, like enabling the dashboard or external api</param>
    /// <returns></returns>
    public static IApplicationBuilder UseVeff(
        this IApplicationBuilder appBuilder,
        Action<VeffConfigBuilder> configBuilder)
    {
        var services = appBuilder.ApplicationServices;

        var connectionFactory = (IVeffDbConnectionFactory)services.GetService(typeof(IVeffDbConnectionFactory))!;
        var containers = ((IEnumerable<IFeatureFlagContainer>)services
                .GetService(typeof(IEnumerable<IFeatureFlagContainer>))!)
            .ToArray();

        EnsureTableExists(connectionFactory);
        SyncFeatureFlagsInDb(connectionFactory, containers);
        SyncValuesFromDb(connectionFactory, containers);
        
        var veffConfigBuilder = new VeffConfigBuilder(appBuilder);

        configBuilder(veffConfigBuilder);
        return appBuilder;
    }

    private static void SyncFeatureFlagsInDb(
        IVeffDbConnectionFactory connectionFactory,
        IEnumerable<IFeatureFlagContainer> containers)
    {
        var featureFlagNames = new List<(string, string)>();
        foreach (var veffContainer in containers)
        {
            var type = veffContainer.GetType();
            var targetType = typeof(Flag);

            type.GetProperties()
                .Where(x => x.PropertyType.IsAssignableTo(targetType))
                .Select(x => ($"{type.Name}.{x.Name}", x.PropertyType.ToString()))
                .ForEach(x => featureFlagNames.Add(x));
        }

        using var conn = connectionFactory.UseConnection();

        conn.SyncFeatureFlags(featureFlagNames);
    }

    private static void SyncValuesFromDb(
        IVeffDbConnectionFactory connectionFactory,
        IEnumerable<IFeatureFlagContainer> veffContainers)
    {
        using var conn = connectionFactory.UseConnection();

        conn.SyncValuesFromDb(veffContainers);
    }

    private static void EnsureTableExists(
        IVeffDbConnectionFactory connFactory)
    {
        using var conn = connFactory.UseConnection();

        conn.EnsureTablesExists();
    }
}