using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Veff.Extensions;
using Veff.Requests;

namespace Veff;

public static class VeffDashboardApplicationBuilder
{
    private static string _path = "";
    private const string ApiPath = "/veff_internal_api";
    private static string GetApiPath => $"{_path}{ApiPath}";
        
    public static IApplicationBuilder UseVeffDashboard(
        this IApplicationBuilder appBuilder,
        string path = "/veff-dashboard")
    {
        _path = path.EnsureStartsWith("/");

        var services = appBuilder.ApplicationServices;
        var authorizers =
            services.GetService(typeof(IEnumerable<IVeffDashboardAuthorizer>)) as IVeffDashboardAuthorizer[]
            ?? Array.Empty<IVeffDashboardAuthorizer>();
            
        MapVeffDashboardIndex(appBuilder, authorizers);
        MapInit(appBuilder, authorizers, services);
        MapUpdateCall(appBuilder, authorizers, services);
            
        return appBuilder;
    }

    private static void MapVeffDashboardIndex(
        IApplicationBuilder appBuilder,
        IEnumerable<IVeffDashboardAuthorizer> authorizers)
    {
        appBuilder.Map(_path, app => app.Run(async context =>
        {
            if (await authorizers.IsAuthorized(context))
            {
                context.Response.ContentType = "text/html";

                var assembly = typeof(VeffDbModel).Assembly;
                await using var manifestResourceStream = assembly.GetManifestResourceStream("Veff.html.templates.inlined.html")!;
                using var reader = new StreamReader(manifestResourceStream);

                await context.Response.WriteAsync(await reader.ReadToEndAsync());
            }
        }));
    }
        
    private static void MapInit(
        IApplicationBuilder appBuilder,
        IEnumerable<IVeffDashboardAuthorizer> authorizers,
        IServiceProvider services)
    {
        appBuilder.Map($"{GetApiPath}/init", app => app.Run(async context =>
        {
            if (await authorizers.IsAuthorized(context))
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(await GetAll(services));
            }
        }));
    }

    private static void MapUpdateCall(
        IApplicationBuilder appBuilder,
        IVeffDashboardAuthorizer[] authorizers,
        IServiceProvider services)
    {
        appBuilder.Map($"{GetApiPath}/update", app => app.Run(async context =>
        {
            if (await authorizers.IsAuthorized(context))
            {
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = 201;
                var update = await Update(services, context);
                await context.Response.WriteAsync(update);
            }
        }));
    }


    private static async Task<string> Update(
        IServiceProvider services,
        HttpContext httpContext)
    {
        var obj = await JsonSerializer.DeserializeAsync<FeatureFlagUpdate>(httpContext.Request.Body);
        SaveUpdate(obj, services);

        return "ok";
    }

    private static void SaveUpdate(
        FeatureFlagUpdate? featureFlagUpdate,
        IServiceProvider serviceProvider)
    {
        if (featureFlagUpdate is null) return;

        var veffSqlConnectionFactory = (IVeffDbConnectionFactory)serviceProvider.GetService(typeof(IVeffDbConnectionFactory))!;
        using var conn = veffSqlConnectionFactory.UseConnection();
        conn.SaveUpdate(featureFlagUpdate);
    }

    private static async Task<string> GetAll(
        IServiceProvider services)
    {
        var veffSqlConnectionFactory = (IVeffDbConnectionFactory) services.GetService(typeof(IVeffDbConnectionFactory))!;
        using var conn = veffSqlConnectionFactory.UseConnection();

        var featureContainerViewModel = await conn.GetAll();

        return JsonSerializer.Serialize(featureContainerViewModel);
    }
}