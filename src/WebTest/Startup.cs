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
        public Startup(
            IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(
            IServiceCollection services)
        {
            services.AddVeff(settings =>
            {
                settings
                    .WithSqlServer(Configuration.GetConnectionString("SqlDb")!)
                    .AddFeatureFlagContainers(new NewStuffFeatures())
                    .AddCacheExpiryTime(TimeSpan.FromSeconds(30));
            });

            services.AddSingleton<IMySuperService, MySuperService>();
            services.AddSingleton<IVeffDashboardAuthorizer, MyCustomAuthorizer>();
            services.AddControllers();
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseCors(builder => builder.AllowAnyOrigin());
            app.UseVeff();
            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }

    public class NewStuffFeatures : IFeatureFlagContainer
    {
        public BooleanFlag Hello { get; } = BooleanFlag.Empty;
        public BooleanFlag CanUseEmails { get; } =  BooleanFlag.Empty;
        public PercentageFlag SometimesIWork { get; } =  PercentageFlag.Empty;
        public StringEqualsFlag Baz111 { get; } =  StringEqualsFlag.Empty;
        public StringContainsFlag Baz333 { get; } =  StringContainsFlag.Empty;
        public StringStartsWithFlag Baz555 { get; } =  StringStartsWithFlag.Empty;
        public StringEndsWithFlag EndsWith { get; } =  StringEndsWithFlag.Empty;
    }

    public class MyCustomAuthorizer : IVeffDashboardAuthorizer
    {
        public Task<bool> IsAuthorized(
            HttpContext context)
        {
            return Task.FromResult(true); // context.Connection.RemoteIpAddress?.ToString() == "0.0.0.0";
        }
    }
}