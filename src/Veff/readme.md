normal usage of Veff

```c#

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

app.MapGet("/", ([FromServices] EmailFeatures ef)
=> $"{ef.CanUseEmails.IsEnabled}\nEmailFeatures.SendActualEmails.enabledfor('me') = {ef.SendActualEmails.EnabledFor("me")}");

app.Run();


public class EmailFeatures : IFeatureFlagContainer
{
    public BooleanFlag CanUseEmails { get; } = BooleanFlag.Empty;
    public PercentageFlag IncludeFunnyCatPictures { get; } = PercentageFlag.Empty;
    public StringEqualsFlag SendActualEmails { get; } = StringEqualsFlag.Empty;
}
```