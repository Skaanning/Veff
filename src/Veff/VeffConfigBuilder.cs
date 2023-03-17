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

public class VeffConfigBuilder
{
    private readonly IApplicationBuilder _applicationBuilder;
    private readonly IServiceProvider _serviceProvider;
    private static string _path = "";
    private const string ApiPath = "/veff_internal_api";
    private static string GetApiPath => $"{_path}{ApiPath}";

    internal VeffConfigBuilder(IApplicationBuilder applicationBuilder, IServiceProvider serviceProvider)
    {
        _applicationBuilder = applicationBuilder;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Enables a dashboard where you can configure the registered flags
    /// </summary>
    /// <param name="appBuilder"></param>
    /// <param name="path">Base path for the dashboard</param>
    /// <returns></returns>
    public VeffConfigBuilder UseVeffDashboard(
        string path = "/veff-dashboard")
    {
        _path = path.EnsureStartsWith("/");

        var authorizers =
            _serviceProvider.GetService(typeof(IEnumerable<IVeffDashboardAuthorizer>)) as IVeffDashboardAuthorizer[]
            ?? Array.Empty<IVeffDashboardAuthorizer>();

        MapVeffDashboardIndex(_applicationBuilder, authorizers);
        MapInit(_applicationBuilder, authorizers, _serviceProvider);
        MapUpdateCall(_applicationBuilder, authorizers, _serviceProvider);
        
        return this;
    }
    
    /// <summary>
    /// Creates an http api that exposes all the flags and the possibility of evaluating if enabled/disabled. <br/>
    ///<br/>
    /// GET {basePath} returns all available flags <br/>
    /// GET {basePath/eval} with query params "containername", "name" and optionally "value" evaluates the
    /// given flag against the value and returns the result
    /// </summary>
    /// <param name="appBuilder"></param>
    /// <param name="basePath">Base path for api</param>
    /// <returns></returns>
    public VeffConfigBuilder UseVeffExternalApi(
        string basePath = "/_api/veff")
    {
        basePath = basePath.EnsureStartsWith("/");

        _applicationBuilder.MapWhen(
            context => context.Request.Method.Equals("GET") && context.Request.Path.StartsWithSegments(basePath, StringComparison.OrdinalIgnoreCase), 
            x => x.UseMiddleware<VeffExternalApiMiddleware>(basePath));
        
        return this;
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