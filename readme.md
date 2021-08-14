## Veff -> Very easy feature flags

Well it's easy if you use aspnet core :) 

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddVeff(settings =>
            {
                settings
                    .UseSqlServer(@"Server=localhost,15789;Database=master;User=sa;Password=Your_password123")
                    .AddFeatureFlagContainers(new BasketFeatures(), new FooBarFeatures())
                    .UpdateInBackground(TimeSpan.FromSeconds(30));
            });

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseVeff("/veff");
            
            // all the normal stuff after this :)
            app.UseHttpsRedirection();            
            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
