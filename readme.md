## Veff -> Very easy feature flags


Nuget pkg on the way.

Well it's easy if you use aspnet core :) 

Currently supports 3 types of feature flags. 
StringFlag, PercentFlag and BooleanFlag.

**Boolean** is a simple true/false

**Percent** takes an int from 0 to 100 and returns true or false with that percentage. Could be useful for split testing     

**String** can be assigned multiple strings. Checks if string is present. Case insensitive. Could be useful for emails, auth-roles etc.   

### Setup

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

### Usage

```C#
        private readonly FooBarFeatures _features;

        // works with normal di..
        // also posible to inject IEnumerable<IFeatureContainer> to get all your featureflag containers
        public WeatherForecastController(FooBarFeatures features)
        {
            _features = features;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            // string flag
            if (_features.Baz.EnabledFor("michael"))
                throw new Exception("michael is not allowed");

            // Percent flag
            if (!_features.Bar.IsEnabled)
                throw new Exception("Bar not enabled");
            
            // boolean flag
            if (!_features.Foo.IsEnabled)
                throw new Exception("Foo not enabled");
            
            .....
        }
```


### Dashboard.

Will be automatically updated with values if you add additional "FeatureFlags" or new "FeatureFlags containers".
After you save, your changes will be live within the specified time configured in .UpdateInBackground()

![image](https://user-images.githubusercontent.com/4522165/129459776-629d2312-1829-40ae-b03c-bb855a0528de.png)


