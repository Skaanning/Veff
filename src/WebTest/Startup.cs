using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Veff;
using Veff.Flags;
using WebTest.Controllers;

namespace WebTest
{
    public class Startup
    {
        IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) =>
            Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddVeff(settings =>
            {
                settings
                    .WithSqlServer(@"Server=.\SQLExpress;Database=WebApi;Integrated Security=true;MultipleActiveResultSets=True")
                    .AddFeatureFlagContainers(new FooBarFeatures(), new NewStuffFeatures())
                    .AddUpdateBackgroundService(TimeSpan.FromSeconds(30));
            });

            services.AddSingleton<IMySuperService, MySuperService>();
            services.AddSingleton<IVeffDashboardAuthorizer, MyCustomAuthorizer>();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseVeff();
            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }

    public record FooBarFeatures : IFeatureContainer
    {
        public BooleanFlag Foo {get; init; }
        public PercentFlag Bar {get; init; }
        public  StringFlag Baz {get; init; }
    }

    public class NewStuffFeatures : IFeatureContainer
    {
        public BooleanFlag Hello { get; }
        public BooleanFlag CanUseEmails { get;}
        public BooleanFlag CanUseSomethingElse { get;}
        public PercentFlag Bar123 { get; }
        public PercentFlag Bar333 { get; }
        public PercentFlag Bar555 { get; }
        public StringFlag Baz111 { get; }
        public StringFlag Baz333 { get; set; }
        public StringFlag Baz555 { get; set; }
    }

    public class MyCustomAuthorizer : IVeffDashboardAuthorizer
    {
        public Task<bool> IsAuthorized(HttpContext context)
        {
            return Task.FromResult(true); // context.Connection.RemoteIpAddress?.ToString() == "0.0.0.0";
        }
    }
}