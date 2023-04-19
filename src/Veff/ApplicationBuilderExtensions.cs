using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public static async Task<IApplicationBuilder> UseVeff(
        this IApplicationBuilder appBuilder,
        Action<VeffConfigBuilder> configBuilder)
    {
        var services = appBuilder.ApplicationServices;

        var connectionFactory = (IVeffDbConnectionFactory)services.GetService(typeof(IVeffDbConnectionFactory))!;
        var containers = ((IEnumerable<IFeatureFlagContainer>)services
                .GetService(typeof(IEnumerable<IFeatureFlagContainer>))!)
            .ToArray();

        await EnsureTableExists(connectionFactory);
        await SyncFeatureFlagsInDb(connectionFactory, containers);
        await SyncValuesFromDb(connectionFactory, containers);
        
        var veffConfigBuilder = new VeffConfigBuilder(appBuilder);

        configBuilder(veffConfigBuilder);
        return appBuilder;
    }

    private static async Task SyncFeatureFlagsInDb(
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

        await conn.SyncFeatureFlags(featureFlagNames);
    }

    private static async Task SyncValuesFromDb(
        IVeffDbConnectionFactory connectionFactory,
        IEnumerable<IFeatureFlagContainer> veffContainers)
    {
        using var conn = connectionFactory.UseConnection();

        await conn.SyncValuesFromDb(veffContainers);
    }

    private static async Task EnsureTableExists(
        IVeffDbConnectionFactory connFactory)
    {
        using var conn = connFactory.UseConnection();

        await conn.EnsureTablesExists();
    }
}