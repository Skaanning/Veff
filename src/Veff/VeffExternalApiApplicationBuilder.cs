using System;
using Microsoft.AspNetCore.Builder;
using Veff.Extensions;

namespace Veff;

public static class VeffExternalApiApplicationBuilder
{

    /// <summary>
    /// Creates a http api that exposes all the flags and their states. 
    /// </summary>
    /// <param name="appBuilder"></param>
    /// <param name="basePath">Base path for api</param>
    /// <returns></returns>
    public static IApplicationBuilder UseVeffExternalApi(
        this IApplicationBuilder appBuilder,
        string basePath = "/_api/veff")
    {
        basePath = basePath.EnsureStartsWith("/");

        appBuilder.MapWhen(
            context => context.Request.Method.Equals("GET") && context.Request.Path.StartsWithSegments(basePath, StringComparison.OrdinalIgnoreCase), 
            x => x.UseMiddleware<VeffExternalApiMiddleware>(basePath));
        
        return appBuilder;
    }
}