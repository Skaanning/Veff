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
using Veff.Flags;

namespace Veff
{
    public static class ApplicationBuilderExtensions
    {
        private static readonly string Response;

        static ApplicationBuilderExtensions()
        {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var textPath = Path.Combine(assemblyDirectory!, "Inlined.html");

            Response = File.ReadAllText(textPath);
        }

        public static IApplicationBuilder UseVeff(this IApplicationBuilder appBuilder,
            string path = "/veff")
        {
            path = EnsureStartsWith(path, "/");

            var services = appBuilder.ApplicationServices;

            var apiPath = "/veff_internal_api";
            appBuilder.Map($"{apiPath}/init", app => app.Run(async context =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(await GetAll(services));
            }));
            
            appBuilder.Map($"{apiPath}/update", app => app.Run(async context =>
            {
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = 201;
                var update = await Update(services, context);
                await context.Response.WriteAsync(update);
            }));

            appBuilder.Map(path, app => app.Run(async context =>
            {
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(Response);
            }));

            return appBuilder;
        }

        private static async Task<string> Update(IServiceProvider services, HttpContext httpContext)
        {
            using var stream = new StreamReader(httpContext.Request.Body);
            var body = await stream.ReadToEndAsync();
            var obj = JsonConvert.DeserializeObject<FeatureFlagUpdate>(body);
            await SaveUpdate(obj, services);
            
            return "ok";
        }

        private static async Task SaveUpdate(FeatureFlagUpdate featureFlagUpdate, IServiceProvider serviceProvider)
        {
            // TODO fix this
            var conn = serviceProvider.GetService(typeof(SqlConnection)) as SqlConnection;
            await conn!.OpenAsync();

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
            var strings = featureFlagUpdate.Strings.Replace('\n', ';');
            sqlCommand.Parameters.Add("@Strings", SqlDbType.NVarChar).Value = strings ?? "";
            sqlCommand.Parameters.Add("@Id", SqlDbType.Int).Value = featureFlagUpdate.Id;

            sqlCommand.ExecuteNonQuery();
            
            await conn.CloseAsync();
        }

        private static async Task<string> GetAll(IServiceProvider services)
        {
            // TODO fix this
            var conn = services.GetService(typeof(SqlConnection)) as SqlConnection;
            
            await conn!.OpenAsync();
            var sqlCommand = new SqlCommand(@"
SELECT [Id], [Name], [Description], [Percent], [Type], [Strings]
FROM Veff_FeatureFlags
", conn);
            await using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();

            var veffDbModels = new List<VeffDbModel>();
            while (await sqlDataReader.ReadAsync())
            {
                veffDbModels.Add(new VeffDbModel(
                    sqlDataReader.GetInt32(0),
                    sqlDataReader.GetString(1), 
                    sqlDataReader.GetString(2), 
                    sqlDataReader.GetInt32(3), 
                    sqlDataReader.GetString(4),
                    sqlDataReader.GetString(5)));
            }
            conn.Close();

            var array = veffDbModels.Select(x => x.AsImpl())
                .Select(x => new FeatureFlagViewModel(x))
                .ToArray();
            
            return JsonConvert.SerializeObject(new FeatureContainerViewModel(array));
        }
        
        
        private static string EnsureStartsWith(string pathMatch, string s)
        {
            return pathMatch.StartsWith(s) ? pathMatch : $"{s}{pathMatch}";
        }
    }

    public class FeatureContainerViewModel
    {
        public FeatureContainerViewModel(FeatureFlagViewModel[] flags)
        {
            Flags = flags;
        }

        public FeatureFlagViewModel[] Flags { get; set; }
    }

    public class FeatureFlagUpdate
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Strings { get; set; }
        public int Percent { get; set; }
    }

    public class FeatureFlagViewModel
    {
        public FeatureFlagViewModel(Flag flag)
        {
            if (flag is BooleanFlag b)
            {
                ContainerName = b.Name.Split('.')[0];
                Name = b.Name.Split('.')[1];
                Enabled = b.IsEnabled;
                Type = nameof(BooleanFlag);
                Description = b.Description;
                Id = b.Id;
            }
            if (flag is StringFlag f)
            {
                ContainerName = f.Name.Split('.')[0];
                Name = f.Name.Split('.')[1];
                Strings = f.Values.ToArray();
                Type = nameof(StringFlag);
                Description = f.Description;
                Id = f.Id;
            }
            if (flag is PercentFlag p)
            {
                ContainerName = p.Name.Split('.')[0];
                Name = p.Name.Split('.')[1];
                Percent = p.Percent;
                Type = nameof(PercentFlag);
                Description = p.Description;
                Id = p.Id;
            }
        }

        public int Id { get; }
        public string ContainerName { get; }
        public string Name { get; }
        public string Description { get; }
        public string Type { get; }
        public int Percent { get; }
        public bool Enabled { get; }
        public string[] Strings { get; }
    }
}