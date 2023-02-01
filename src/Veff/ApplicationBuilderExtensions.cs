using System;
using System.Collections.Generic;
using System.IO;
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
        private const string ApiPath = "/veff_internal_api";

        public static IApplicationBuilder UseVeff(
            this IApplicationBuilder appBuilder,
            string path = "/veff")
        {
            path = EnsureStartsWith(path, "/");

            var services = appBuilder.ApplicationServices;
            var authorizers = services.GetService(typeof(IEnumerable<IVeffDashboardAuthorizer>)) as IVeffDashboardAuthorizer[] 
                              ?? Array.Empty<IVeffDashboardAuthorizer>();
            
            MapVeffDashboardIndex(appBuilder, path, authorizers);
            MapInit(appBuilder, authorizers, services);
            MapUpdateCall(appBuilder, authorizers, services);
            
            return appBuilder;
        }

        private static void MapVeffDashboardIndex(
            IApplicationBuilder appBuilder,
            string path,
            IEnumerable<IVeffDashboardAuthorizer> authorizers)
        {
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
        }
        
        private static void MapInit(
            IApplicationBuilder appBuilder,
            IEnumerable<IVeffDashboardAuthorizer> authorizers,
            IServiceProvider services)
        {
            appBuilder.Map($"{ApiPath}/init", app => app.Run(async context =>
            {
                if (await IsAuthorized(authorizers, context))
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
            appBuilder.Map($"{ApiPath}/update", app => app.Run(async context =>
            {
                if (await IsAuthorized(authorizers, context))
                {
                    context.Response.ContentType = "text/plain";
                    context.Response.StatusCode = 201;
                    var update = await Update(services, context);
                    await context.Response.WriteAsync(update);
                }
            }));
        }
        
        private static async Task<bool> IsAuthorized(
            IEnumerable<IVeffDashboardAuthorizer> authorizers,
            HttpContext context)
        {
            var authorized = true;
            foreach (var authorizer in authorizers)
            {
                authorized = authorized && await authorizer.IsAuthorized(context);
            }

            if (!authorized)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("unauthorized");
            }
            
            return authorized;
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