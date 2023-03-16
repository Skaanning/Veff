using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Veff.Extensions;
using Veff.External;
using Veff.Flags;

namespace Veff;

// ReSharper disable once ClassNeverInstantiated.Global
public class VeffExternalApiMiddleware
{
    private readonly string _basePath;

    public VeffExternalApiMiddleware(RequestDelegate _, string basePath)
    {
        _basePath = basePath;
    }

    // ReSharper disable once UnusedMember.Global
    public async Task InvokeAsync(
        HttpContext context,
        IVeffDbConnectionFactory veffDbConnectionFactory,
        IEnumerable<IFeatureFlagContainer> containers,
        IEnumerable<IVeffExternalApiAuthorizer> authorizers)
    {
        var isAuthorized = await CheckAuthorized(context, authorizers);
        if (!isAuthorized) 
            return;

        if (await HandleFeatureRequest(context, _basePath, containers)) 
            return;
        
        await HandleGetAll(context, veffDbConnectionFactory);
    }

    private static async Task<bool> HandleFeatureRequest(HttpContext context, string basePath, IEnumerable<IFeatureFlagContainer> containers)
    {
        if (!context.Request.Path.StartsWithSegments($"{basePath}/eval", StringComparison.OrdinalIgnoreCase)) return false;
        
        var req = Input.FromQueryCollection(context.Request.Query);

        if (req is null) 
            return await SetBadRequest(context);
        
        var container = containers.FirstOrDefault(x => x.GetType().Name.Equals(req.ContainerName));
        if (container is null) 
            return await SetBadRequest(context, req);

        var type = container.GetType();
        var targetType = typeof(Flag);

        var prop = type.GetProperties()
            .Where(x => x.PropertyType.IsAssignableTo(targetType))
            .FirstOrDefault(x => x.Name.Equals(req.Name, StringComparison.OrdinalIgnoreCase));

        if (prop is null) 
            return await SetBadRequest(context, req);
        
        var untypedFlag = prop.GetValue(container);

        try
        {
            var result = untypedFlag switch
            {
                PercentageFlag p => int.TryParse(req.Value, out var n)
                    ? p.EnabledFor(n)
                    : Guid.TryParse(req.Value, out var guid)
                        ? p.EnabledFor(guid)
                        : throw new ArgumentOutOfRangeException("Value",
                            "Value for PercentageFlag should be either a Guid or an int"),
                BooleanFlag b => b.IsEnabled,
                StringEqualsFlag f => f.EnabledFor(req.Value),
                _ => throw new ArgumentOutOfRangeException("untypedFlag", $"unknown flagtype {untypedFlag?.GetType()}")
            };

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                $$"""
{
    "result": {{result}},
    "property": "{{req.ContainerName}}.{{req.Name}}", 
    "evaluatedOn": "{{req.Value}}"
}
""");
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return await SetBadRequest(context, e: exception);
        }
        
        return true;
    }

    private static async Task<bool> SetBadRequest(HttpContext httpContext, Input? req = null, ArgumentOutOfRangeException? e = null)
    {
        httpContext.Response.ContentType = "text/plain";
        httpContext.Response.StatusCode = 400;

        if (req is null && e is null)
            await httpContext.Response.WriteAsync("Cannot parse the query params. Use 'containername', 'name' and optionally 'value'");
        else if (e is not null)
            await httpContext.Response.WriteAsync($"{e.Message}");
        else if (req is not null)
            await httpContext.Response.WriteAsync($"Unable to find container {req.ContainerName} with flag {req.Name}");
        else
            await httpContext.Response.WriteAsync("unknown err");
        
        return true;
    }

    private static async Task HandleGetAll(HttpContext context, IVeffDbConnectionFactory veffDbConnectionFactory)
    {
        var all = await veffDbConnectionFactory.UseConnection().GetAll();
        var featureFlagVms = all.Flags?.SelectToArray(FeatureFlagVm.FromFeatureFlagViewModel);

        context.Response.StatusCode = 200;
        context.Response.ContentType = "application/json";

        var serialize = JsonSerializer.Serialize(featureFlagVms);
        await context.Response.WriteAsync(serialize);
    }

    private static async Task<bool> CheckAuthorized(
        HttpContext context,
        IEnumerable<IVeffExternalApiAuthorizer> authorizers)
    {
        var isAuthorized = await authorizers.IsAuthorized(context);
        if (isAuthorized) return true;

        context.Response.StatusCode = 401;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync("no you are not allowed :(");
        return false;
    }

    private record Input(string ContainerName, string Name, string Value)
    {
        public static Input? FromQueryCollection(IQueryCollection collection)
        {
            var name = collection.TryGetValue("name", out var n) ? n.FirstOrDefault() ?? "" : "";
            var containerName = collection.TryGetValue("containerName", out var c) ? c.FirstOrDefault() ?? "" : "";
            var value = collection.TryGetValue("value", out var s) ? s.FirstOrDefault() ?? "" : "";
            
            var input = new Input(containerName, name, value);

            return input.IsNotValid() ? null : input;
        }

        private bool IsNotValid()
        {
            return string.IsNullOrWhiteSpace(ContainerName)
                   || string.IsNullOrWhiteSpace(Name);
        }
    }
}