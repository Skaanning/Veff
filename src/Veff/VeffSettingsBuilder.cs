﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Veff.Extensions;
using Veff.Flags;

namespace Veff;

public class VeffSettingsBuilder : IVeffSettingsBuilder
{
    protected IVeffDbConnectionFactory? VeffSqlConnectionFactory;
    protected readonly IServiceCollection ServiceCollection;

    protected VeffSettingsBuilder(
        IServiceCollection serviceCollection)
    {
        ServiceCollection = serviceCollection;
    }

    public IVeffSettingsBuilder AddFeatureFlagContainers(
        params IFeatureFlagContainer[] containers)
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

        SyncFeatureFlagsInDb(featureFlagNames);
        SyncValuesFromDb(containers);

        containers.ForEach(x => ServiceCollection.AddSingleton(x.GetType(), x));
        containers.ForEach(x => ServiceCollection.AddSingleton(x));

        return this;
    }

    public IVeffSettingsBuilder AddCacheExpiryTime(
        TimeSpan cacheExpiry)
    {
        VeffSqlConnectionFactory!.CacheExpiry = cacheExpiry;
        ServiceCollection.Replace(new ServiceDescriptor(typeof(IVeffDbConnectionFactory), _ => VeffSqlConnectionFactory, ServiceLifetime.Transient));
        return this;
    }

    private void SyncFeatureFlagsInDb(
        IEnumerable<(string Name, string Type)> featureFlagNames)
    {
        using var conn = VeffSqlConnectionFactory!.UseConnection();

        conn.SyncFeatureFlags(featureFlagNames);
    }

    private void SyncValuesFromDb(
        IEnumerable<IFeatureFlagContainer> veffContainers)
    {
        using var conn = VeffSqlConnectionFactory!.UseConnection();

        conn.SyncValuesFromDb(veffContainers);
    }

    protected static void EnsureTableExists(
        IVeffDbConnectionFactory connFactory)
    {
        using var conn = connFactory.UseConnection();

        conn.EnsureTablesExists();
    }
}