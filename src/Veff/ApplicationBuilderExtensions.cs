using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Veff.Internal;
using Veff.Internal.Requests;
using Veff.Internal.Responses;

namespace Veff
{
    public static class ApplicationBuilderExtensions
    {
        private static readonly string Response;

        static ApplicationBuilderExtensions()
        {
            string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string textPath = Path.Combine(assemblyDirectory!, "Inlined.html");

            Response = File.ReadAllText(textPath);
        }

        public static IApplicationBuilder UseVeff(
            this IApplicationBuilder appBuilder,
            string path = "/veff")
        {
            path = EnsureStartsWith(path, "/");

            IServiceProvider services = appBuilder.ApplicationServices;
            IVeffDashboardAuthorizer[] authorizers
                = services.GetService(typeof(IEnumerable<IVeffDashboardAuthorizer>)) as IVeffDashboardAuthorizer[]
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
                    string update = await Update(services, context);
                    await context.Response.WriteAsync(update);
                }
            }));

            appBuilder.Map(path, app => app.Run(async context =>
            {
                if (await IsAuthorized(authorizers, context))
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(Response);
                }
            }));

            return appBuilder;
        }

        private static async Task<bool> IsAuthorized(
            IVeffDashboardAuthorizer[] authorizers,
            HttpContext context)
        {
            var authorized = true;
            foreach (IVeffDashboardAuthorizer authorizer in authorizers)
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
            string body = await stream.ReadToEndAsync();
            var obj = JsonConvert.DeserializeObject<FeatureFlagUpdate>(body);
            SaveUpdate(obj, services);

            return "ok";
        }

        private static void SaveUpdate(
            FeatureFlagUpdate featureFlagUpdate,
            IServiceProvider serviceProvider)
        {
            var veffSqlConnectionFactory
                = serviceProvider.GetService(typeof(IVeffSqlConnectionFactory)) as IVeffSqlConnectionFactory;
            using SqlConnection conn = veffSqlConnectionFactory!.UseConnection();

            var sqlCommand = new SqlCommand(@"
UPDATE [dbo].[Veff_FeatureFlags]
   SET [Description] = @Description
      ,[Percent] = @Percent
      ,[Strings] = @Strings
 WHERE 
    [Id] = @Id 
", conn);

            sqlCommand.Parameters.Add("@Description", SqlDbType.NVarChar).Value = featureFlagUpdate.Description ?? "";
            sqlCommand.Parameters.Add("@Percent", SqlDbType.Int).Value = featureFlagUpdate.Percent;
            string strings = featureFlagUpdate.Strings.Replace('\n', ';');
            sqlCommand.Parameters.Add("@Strings", SqlDbType.NVarChar).Value = strings ?? "";
            sqlCommand.Parameters.Add("@Id", SqlDbType.Int).Value = featureFlagUpdate.Id;

            sqlCommand.ExecuteNonQuery();
        }

        private static async Task<string> GetAll(
            IServiceProvider services)
        {
            var veffSqlConnectionFactory
                = services.GetService(typeof(IVeffSqlConnectionFactory)) as IVeffSqlConnectionFactory;
            using SqlConnection conn = veffSqlConnectionFactory!.UseConnection();

            var sqlCommand = new SqlCommand(@"
SELECT [Id], [Name], [Description], [Percent], [Type], [Strings]
FROM Veff_FeatureFlags
", conn);
            await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();

            var veffDbModels = new List<VeffDbModel>();
            while (await sqlDataReader.ReadAsync())
            {
                veffDbModels.Add(new VeffDbModel(
                    sqlDataReader.GetInt32(0),
                    sqlDataReader.GetString(1),
                    sqlDataReader.GetString(2),
                    sqlDataReader.GetInt32(3),
                    sqlDataReader.GetString(4),
                    sqlDataReader.GetString(5),
                    veffSqlConnectionFactory));
            }

            FeatureFlagViewModel[] array = veffDbModels.Select(x => x.AsImpl())
                .Select(x => new FeatureFlagViewModel(x))
                .ToArray();

            return JsonConvert.SerializeObject(new FeatureContainerViewModel(array));
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