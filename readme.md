## Veff -> Very easy feature flags

Well it's easy if you use aspnet core :) 

Currently supports 3 types of feature flags. 
BooleanFlag, StringFlag and PercentageFlag.

- **Boolean** is a simple true/false
- **String** can be assigned multiple strings. Checks if string is present. Case insensitive. Could be useful for emails, auth-roles etc.   
    - can be found in different versions - Equals, Contains, StartsWith and EndsWith.
- **Percentage** set between 0-100%. Will take a Guid or int and give true back x% of the times. The results are repeatable, unless you set a new 'randomSeed' on the flag.  


### Nuget
nuget package `Veff` is the base package, but without any persistence layer.
nuget package `Veff.Sql` references the `Veff` package, and enables using SqlServer as the db.

Additional packages will be made as needed to support other dbs. (expected are Sqlite, Postgres and MySql) 

### Usage

```C#

public class EmailFeatures : IFeatureFlagContainer
{
    public BooleanFlag SendSpamMails { get; } = BooleanFlag.Empty;
    public PercentageFlag IncludeFunnyCatPictures { get; } = PercentageFlag.Empty;
    public StringEqualsFlag SendActualEmails { get; } = StringEqualsFlag.Empty;
}

```


### Setup

```C#

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddVeff(veffBuilder =>
{
    var connectionString = builder.Configuration.GetConnectionString("SqlDb")!;

    veffBuilder
        .WithSqlServer(connectionString, TimeSpan.FromSeconds(30))
        .AddFeatureFlagContainersFromAssembly() // Finds all IFeatureFlagContainer in scanned assemblies 
        .AddDashboardAuthorizersFromAssembly() // Same as above but for IVeffDashboardAuthorizers (only needed if you want to use the dashboard, and hide it behind some authorization)
        .AddExternalApiAuthorizersFromAssembly(); // Same as above but for IVeffExternalApiAuthorizers (only needed if you want to use the external api and hide it behind some auth)
});

var app = builder.Build();

app.UseVeff(s =>
{
    s.UseVeffDashboard(); // setup dashboard where you can manage and edit your feature flags. 
    s.UseVeffExternalApi(); // exposes a http api that allows external services to make use of the feature flags.
});

// just inject your feature flag containers as normal DI
app.MapGet("/", ([FromServices] EmailFeatures ef) 
    => $"{ef.SendSpamEmails.IsEnabled}\n{ef.SendActualEmails.EnabledFor("me")}");

app.Run();

```

#### Note

The FeatureContainers does not care about the data they are initialized with, it will be overridden with whats stored in the db. This also means that flags defaults to false until otherwise specified in the dashboard.


### Dashboard

// TODO 
Needs to be reworked a bit :)

### External API

// TODO
Useful for exposing the feature flags to external services.  

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
            return _fooBarFeatures.Foo.IsEnabled ? "Hello" : "goodbye";
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
