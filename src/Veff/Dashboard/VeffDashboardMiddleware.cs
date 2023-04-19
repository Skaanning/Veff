using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Veff.Persistence;

namespace Veff.Dashboard;

// ReSharper disable once ClassNeverInstantiated.Global
public class VeffDashboardMiddleware
{
    private readonly RequestDelegate _requestDelegate;
    private readonly string _dashboardPath;
    private const string ApiPath = "/veff_internal_api";

    public VeffDashboardMiddleware(RequestDelegate requestDelegate, string dashboardPath)
    {
        _requestDelegate = requestDelegate;
        _dashboardPath = dashboardPath;
    }
    
    // ReSharper disable once UnusedMember.Global
    public async Task InvokeAsync(
        HttpContext context,
        IVeffDbConnectionFactory veffDbConnectionFactory,
        IEnumerable<IVeffDashboardAuthorizer> authorizers)
    {
        if (context.Request.Path.Equals(_dashboardPath, StringComparison.OrdinalIgnoreCase))
        {
            await HandleVeffDashboardIndex(context, authorizers);
            return;
        }

        if (context.Request.Path.Equals($"{ApiPath}/init", StringComparison.OrdinalIgnoreCase))
        {
            await HandleInit(context, authorizers, veffDbConnectionFactory);
            return;
        }
        
        if (context.Request.Path.Equals($"{ApiPath}/update", StringComparison.OrdinalIgnoreCase))
        {
            await HandleUpdateCall(context, authorizers, veffDbConnectionFactory);
            return;
        }
        
        await _requestDelegate(context);
    }
    
    private static async Task HandleVeffDashboardIndex(
        HttpContext context,
        IEnumerable<IVeffDashboardAuthorizer> authorizers)
    {
        if (await authorizers.IsAuthorized(context))
        {
            context.Response.ContentType = "text/html";

            var assembly = typeof(VeffDbModel).Assembly;
            await using var manifestResourceStream = assembly.GetManifestResourceStream("Veff.html.templates.inlined.html")!;
            using var reader = new StreamReader(manifestResourceStream);

            await context.Response.WriteAsync(await reader.ReadToEndAsync());
        }
    }

    private static async Task HandleInit(
        HttpContext context,
        IEnumerable<IVeffDashboardAuthorizer> authorizers,
        IVeffDbConnectionFactory veffDbConnectionFactory)
    {
        if (await authorizers.IsAuthorized(context))
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(await GetAll(veffDbConnectionFactory));
        }
    }

    private static async Task HandleUpdateCall(
        HttpContext context,
        IEnumerable<IVeffDashboardAuthorizer> authorizers,
        IVeffDbConnectionFactory veffDbConnectionFactory)
    {
        if (await authorizers.IsAuthorized(context))
        {
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 201;
            var update = await Update(veffDbConnectionFactory, context);
            await context.Response.WriteAsync(update);
        }
    }
    
    private static async Task<string> Update(
        IVeffDbConnectionFactory veffDbConnectionFactory,
        HttpContext httpContext)
    {
        var obj = await JsonSerializer.DeserializeAsync<FeatureFlagUpdate>(httpContext.Request.Body);
        await SaveUpdate(obj, veffDbConnectionFactory);

        return "ok";
    }

    private static async Task SaveUpdate(
        FeatureFlagUpdate? featureFlagUpdate,
        IVeffDbConnectionFactory veffDbConnectionFactory)
    {
        if (featureFlagUpdate is null) return;

        using var conn = veffDbConnectionFactory.UseConnection();
        await conn.SaveUpdate(featureFlagUpdate);
    }

    private static async Task<string> GetAll(
        IVeffDbConnectionFactory veffDbConnectionFactory)
    {
        using var conn = veffDbConnectionFactory.UseConnection();

        var featureContainerViewModel = await conn.GetAll();

        return JsonSerializer.Serialize(featureContainerViewModel);
    }
}