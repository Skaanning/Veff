using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
                    .UseSqlServer(@"Server=localhost,15789;Database=master;User=sa;Password=Your_password123")
                    .AddFeatureFlagContainers(new FooBarFeatures(), new NewStuffFeatures())
                    .UpdateInBackground(TimeSpan.FromSeconds(30));
            });

            services.AddSingleton<IMySuperService, MySuperService>();

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
    
    public class FooBarFeatures : IFeatureContainer
    {
        public BooleanFlag Foo { get; set;}
        public PercentFlag Bar { get; set; }
        public StringFlag Baz { get; set; }
    }
    
    public class NewStuffFeatures : IFeatureContainer
    {
        public BooleanFlag Hello { get; set;}
        public BooleanFlag CanUseEmails { get; set;}
        public BooleanFlag CanUseSomethingElse { get; set;}
        public PercentFlag Bar123 { get; set; }
        public PercentFlag Bar333 { get; set; }
        public PercentFlag Bar555 { get; set; }
        public StringFlag Baz111 { get; set; }
        public StringFlag Baz333 { get; set; }
        public StringFlag Baz555 { get; set; }
    }
}