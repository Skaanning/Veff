## Veff -> Very easy feature flags

Well it's easy if you use aspnet core :) 

Currently supports 3 types of feature flags. 
StringFlag, PercentFlag and BooleanFlag.

**Boolean** is a simple true/false  
**Percent** takes an int from 0 to 100 and returns true or false with that percentage.   
**String** can be assigned multiple strings. Checks if string is present.   


```C#
        public void ConfigureServices(IServiceCollection services)
        {
            // Add veff to the project. Currently only works with sqlserver.    
            services.AddVeff(settings =>
            {
                settings
                    .UseSqlServer(@"Server=localho.....") // Saves the featureflags in table dbo.Veff_FeatureFlags. Will be auto created if not there.
                    .AddFeatureFlagContainers(new FooBarFeatures()) // add your feature flag containers here
                    .UpdateInBackground(TimeSpan.FromSeconds(30)); // background job runs every 30 sec, updates the singleton feature containers with values from db.
            });

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // dashboard found on this url, auth to come..
            app.UseVeff("/veff");
            
            // all the normal stuff after this :)
            app.UseHttpsRedirection();            
            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
        
        // example of a feature flag container. 
        public class FooBarFeatures : IFeatureContainer
        {
                public BooleanFlag Foo { get;  private set;}
                public PercentFlag Bar { get; private set; }
                public StringFlag Baz { get; private set; }
        }
```


Example of dashboard.
will be automatically updated with values if you add additional "FeatureFlags" or new "FeatureFlags containers"
![image](https://user-images.githubusercontent.com/4522165/129459776-629d2312-1829-40ae-b03c-bb855a0528de.png)

