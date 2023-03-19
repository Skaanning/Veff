using System;
using Microsoft.AspNetCore.Builder;
using Veff.Dashboard;
using Veff.Extensions;
using Veff.ExternalApi;

namespace Veff;

public class VeffConfigBuilder
{
    private readonly IApplicationBuilder _applicationBuilder;

    protected internal VeffConfigBuilder(IApplicationBuilder applicationBuilder)
    {
        _applicationBuilder = applicationBuilder;
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
        path = path.EnsureStartsWith("/");

        _applicationBuilder.UseMiddleware<VeffDashboardMiddleware>(path);
        return this;
    }

    /// <summary>
    /// Creates an http api that exposes all the flags and the possibility of evaluating if enabled/disabled. <br/>
    ///<br/>
    /// GET 'basePath' returns all available flags <br/>
    /// GET 'basePath/eval' with query params "containername", "name" and optionally "value" evaluates the
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
}