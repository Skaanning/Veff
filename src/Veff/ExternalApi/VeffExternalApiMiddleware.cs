using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Veff.Abstractions;
using Veff.Extensions;
using Veff.Flags;

namespace Veff.ExternalApi;

// ReSharper disable once ClassNeverInstantiated.Global
internal class VeffExternalApiMiddleware
{
    private readonly string _basePath;

    public VeffExternalApiMiddleware(RequestDelegate _, string basePath)
    {
        _basePath = basePath;
    }

    // ReSharper disable once UnusedMember.Global
    public async Task InvokeAsync(HttpContext context, IEnumerable<IFeatureFlagContainer> containers, IEnumerable<IVeffExternalApiAuthorizer> authorizers)
    {
        if (!await CheckAuthorized(context, authorizers)) 
            return;

        var featureFlagContainers = containers as IFeatureFlagContainer[] ?? containers.ToArray();
        
        if (await HandleGetContainer(context, featureFlagContainers))
            return;
        
        if (await HandleFeatureRequest(context, _basePath, featureFlagContainers)) 
            return;
        
        await HandleGetAll(context, featureFlagContainers);
    }

    private static async Task<bool> HandleFeatureRequest(
        HttpContext context,
        string basePath,
        IFeatureFlagContainer[] containers)
    {
        if (!context.Request.Path.StartsWithSegments($"{basePath}/eval", StringComparison.OrdinalIgnoreCase))
            return false;

        var req = Input.FromQueryCollection(context.Request.Query);

        if (req is null) 
            return await SetBadRequest(context);
        
        var container = containers.FirstOrDefault(x => x.GetType().Name.Equals(req.ContainerName, StringComparison.OrdinalIgnoreCase));
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
                DateFlag f => f.IsEnabledAfter(DateTime.Parse(req.Value)),
                _ => throw new ArgumentOutOfRangeException("untypedFlag", $"unknown flagtype {untypedFlag?.GetType()}")
            };

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = MediaTypeNames.Application.Json;
            
            var response = new FeatureEvaluation(result, $"{req.ContainerName}.{req.Name}", $"{req.Value}");
           
            var responseString = JsonSerializer.Serialize(response, JsonSerializerOptions.Default);
            
            await context.Response.WriteAsync(responseString);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return await SetBadRequest(context, e: exception);
        }
        
        return true;
    }

    private static async Task<bool> SetBadRequest(HttpContext httpContext, Input? req = null, ArgumentOutOfRangeException? e = null)
    {
        httpContext.Response.ContentType = MediaTypeNames.Text.Plain;
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        if (req is null && e is null)
            await httpContext.Response.WriteAsync("Cannot parse the query params. Use 'containername', 'name' and optionally 'value'");
        else if (e is not null)
            await httpContext.Response.WriteAsync($"{e.Message}");
        else if (req is not null)
            await httpContext.Response.WriteAsync($"Unable to find container {req.ContainerName} with flag {req.Name}");
        else
            await httpContext.Response.WriteAsync("Unknown error");
        
        return true;
    }

    private static async Task HandleGetAll(HttpContext context, IFeatureFlagContainer[] containers)
    {
        var featureFlags = FeatureFlagContainerExtensions.FromFeatureFlagContainers(containers);
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var serialize = JsonSerializer.Serialize(featureFlags, JsonSerializerOptions.Default);
        await context.Response.WriteAsync(serialize);
    }
    
    // csharp
    private static string? GetContainerNameFromPath(HttpContext context, string basePath)
    {
        // Check and capture the remainder of the path after "{basePath}/container"
        if (!context.Request.Path.StartsWithSegments($"{basePath}/container", StringComparison.OrdinalIgnoreCase, out var remaining))
            return null;
    
        var remainder = remaining.Value?.Trim('/') ?? string.Empty;
        if (string.IsNullOrEmpty(remainder))
            return null;
    
        // First segment is the container name
        return remainder.Split('/', StringSplitOptions.RemoveEmptyEntries)[0];
    }

    private async Task<bool> HandleGetContainer(HttpContext context, IFeatureFlagContainer[] containers)
    {
        var containerName = GetContainerNameFromPath(context, _basePath);
        if (containerName is null)
            return false;
    
        // Example: find container by type name (case-insensitive)
        var container = containers.FirstOrDefault(c =>
            c.GetType().Name.Equals(containerName, StringComparison.OrdinalIgnoreCase));
    
        if (container is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = MediaTypeNames.Text.Plain;
            await context.Response.WriteAsync($"Container '{containerName}' not found");
            return false;
        }
    
        var featureFlags = FeatureFlagContainerExtensions.FromFeatureFlagContainers(container);
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = MediaTypeNames.Application.Json;
        var serialize = JsonSerializer.Serialize(featureFlags, JsonSerializerOptions.Default);
        await context.Response.WriteAsync(serialize);
        return true;
    }

    private static async Task<bool> CheckAuthorized(
        HttpContext context,
        IEnumerable<IVeffExternalApiAuthorizer> authorizers)
    {
        var isAuthorized = await authorizers.IsAuthorized(context);
        if (isAuthorized) return true;

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = MediaTypeNames.Text.Plain;
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