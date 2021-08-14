using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Veff;
using Veff.Flags;

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
                    .AddFeatureFlagContainers(new BasketFeatures(), new FooBarFeatures())
                    .UpdateInBackground(TimeSpan.FromSeconds(30));
            });
            services.AddCors();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseVeff();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }

    public class BasketFeatures : IFeatureContainer
    {
        public BooleanFlag Hello { get;  private set;}
        public PercentFlag Ello { get; private set; }
        public StringFlag Abc { get; private set; }
    }    
    
    public class FooBarFeatures : IFeatureContainer
    {
        public BooleanFlag Foo { get;  private set;}
        public PercentFlag Bar { get; private set; }
        public StringFlag Baz { get; private set; }
    }
}