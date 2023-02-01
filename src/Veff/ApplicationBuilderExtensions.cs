using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Veff.Internal;
using Veff.Internal.Requests;

namespace Veff
{
    public static class ApplicationBuilderExtensions
    {
        // private static readonly string Response;

        // static ApplicationBuilderExtensions()
        // {
        //     var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        //     var textPath = Path.Combine(assemblyDirectory, "Inlined.html");
        //
        //     Response = File.ReadAllText(textPath);
        // }

        public static IApplicationBuilder UseVeff(
            this IApplicationBuilder appBuilder,
            string path = "/veff")
        {
            path = EnsureStartsWith(path, "/");

            var services = appBuilder.ApplicationServices;
            var authorizers = services.GetService(typeof(IEnumerable<IVeffDashboardAuthorizer>)) as IVeffDashboardAuthorizer[]
                              ?? Array.Empty<IVeffDashboardAuthorizer>();

            var apiPath = "/veff_internal_api";
            appBuilder.Map($"{apiPath}/init", app => app.Run(async context =>
            {
                if (await IsAuthorized(authorizers, context))
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(await GetAll(services));
                }
            }));

            appBuilder.Map($"{apiPath}/update", app => app.Run(async context =>
            {
                if (await IsAuthorized(authorizers, context))
                {
                    context.Response.ContentType = "text/plain";
                    context.Response.StatusCode = 201;
                    var update = await Update(services, context);
                    await context.Response.WriteAsync(update);
                }
            }));

            appBuilder.Map(path, app => app.Run(async context =>
            {
                if (await IsAuthorized(authorizers, context))
                {
                    context.Response.ContentType = "text/html";
                    
                    var assembly = typeof(VeffDbModel).Assembly;
                    var manifestResourceStream = assembly.GetManifestResourceStream("Veff.html.templates.dashboard.html")!;
                    var reader = new StreamReader(manifestResourceStream);
                    
                    await context.Response.WriteAsync(await reader.ReadToEndAsync());
                }
            }));

            return appBuilder;
        }

        private static async Task<bool> IsAuthorized(
            IVeffDashboardAuthorizer[] authorizers,
            HttpContext context)
        {
            var authorized = true;
            foreach (var authorizer in authorizers)
            {
                authorized = authorized && await authorizer.IsAuthorized(context);
            }

            if (authorized)
            {
                return true;
            }

            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("unauthorized");
            return false;
        }

        private static async Task<string> Update(
            IServiceProvider services,
            HttpContext httpContext)
        {
            using var stream = new StreamReader(httpContext.Request.Body);
            var body = await stream.ReadToEndAsync();
            var obj = JsonConvert.DeserializeObject<FeatureFlagUpdate>(body);
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

            return JsonConvert.SerializeObject(featureContainerViewModel);
        }

        private static string EnsureStartsWith(
            string pathMatch,
            string s)
        {
            return pathMatch.StartsWith(s)
                ? pathMatch
                : $"{s}{pathMatch}";
        }
    }
}