## Veff -> Very easy feature flags


`dotnet add package Veff` 

Well it's easy if you use aspnet core :) 

Currently supports 3 types of feature flags. 
StringFlag, PercentFlag and BooleanFlag.

- **Boolean** is a simple true/false
- **Percent** takes an int from 0 to 100 and returns true that percentage of the time. Could be useful for split testing     
- **String** can be assigned multiple strings. Checks if string is present. Case insensitive. Could be useful for emails, auth-roles etc.   

### Setup

```C#
        public void ConfigureServices(IServiceCollection services)
        {
            // Add veff to the project.
            services.AddVeff(settings =>
            {
                settings
                    // Saves the featureflags in table dbo.Veff_FeatureFlags. Will be auto created if not there.
                    .UseSqlServer(@"Server=localho.....") 
                    // add your feature flag containers here
                    .AddFeatureFlagContainers(new FooBarFeatures()) 
                    // background job runs every 30 sec, updates the feature containers with values from the db.
                    .UpdateInBackground(TimeSpan.FromSeconds(30));
            });

            // Create a class implementing IVeffDashboardAuthorizer to add auth before you can acccess the dashboard
            // you can create multiple IVeffDashboardAuthorizers, users will have to fulfil them all to access the dashboard.
            services.AddSingleton<IVeffDashboardAuthorizer, MyCustomAuthorizer>();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // dashboard accessible on this url
            app.UseVeff("/veff");
            
            // all the normal stuff after this :)
            app.UseHttpsRedirection();            
            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
        
        // example of a feature flag container. 
        public class FooBarFeatures : IFeatureContainer
        {
                public BooleanFlag Foo { get; }
                public PercentFlag Bar { get; }
                public StringFlag Baz { get; }
        }

        public record SomeRecordFeature(
                StringFlag UseNewImpl, 
                BooleanFlag UseEmails, 
                BooleanFlag UseSms) : IFeatureContainer;

        public class MyCustomAuthorizer : IVeffDashboardAuthorizer 
        {
            public Task<bool> IsAuthorized(HttpContext context)
            {
                // use the HttpContext to do your auth checks.  
                return Task.FromResult(true); 
            }
        }
```

#### Note

The FeatureContainers does not care what data they are initialized with, it will also take whats stored in the db (meaning flags defaults to false until otherwise specified in the dashboard)


### Usage

```C#
        private readonly FooBarFeatures _features;

        // works with normal di, just inject the concrete class.
        // also posible to inject IEnumerable<IFeatureContainer> to get all your featureflag containers
        public MyFancyController(FooBarFeatures features)
        {
            _features = features;
        }

        [HttpGet]
        public IActionResult Get()
        {
            // string flag
            if (_features.Baz.EnabledFor("michael"))
                return Ok("hello world");

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

Will be automatically updated with new fields if you add additional **FeatureFlags** or new tabs if you add addtional **Containers**.


After you save, your changes will be live within the specified time configured in .UpdateInBackground()

![image](https://user-images.githubusercontent.com/4522165/129459776-629d2312-1829-40ae-b03c-bb855a0528de.png)



### Testing

How do you test class that injects a feature container - since it is not hidden behind an interface?  
Example:

```C#

    public interface IMySuperService
    {
        string DoStuff();
    }


    public class MySuperService : IMySuperService
    {
        private readonly FooBarFeatures _fooBarFeatures;

        public MySuperService(FooBarFeatures fooBarFeatures)
        {
            _fooBarFeatures = fooBarFeatures;
        }
        
        public string DoStuff()
        {
            return _fooBarFeatures.Bar.IsEnabled ? "Hello" : "goodbye";
        }
    }

```

Luckily you can easily test by initializing the FeatureContainer with **MockedFlags**

```C#

    public class MySuperServiceTest
    {
        private readonly MySuperService _sut;

        public MySuperServiceTest()
        {
            var fooBarFeatures = new FooBarFeatures
            {
                Foo = new MockedBooleanFlag(true),
                Bar = new MockedPercentFlag(true),
                Baz = new MockedStringFlag("my@email.com")
            };

            _sut = new MySuperService(fooBarFeatures);
        }
        
        [Fact]
        public void Test1()
        {
            var doStuff = _sut.DoStuff();
            Assert.Equal("Hello", doStuff);
        }
    }


```
