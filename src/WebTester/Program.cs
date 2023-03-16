using Veff;
using Veff.SqlServer;
using WebTester;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddVeff(veffBuilder =>
{
    var connectionString = builder.Configuration.GetConnectionString("SqlDb")!;
    
    veffBuilder
        .WithSqlServer(connectionString)
        .AddFeatureFlagContainers(new NewStuffFeatures())
        .AddCacheExpiryTime(TimeSpan.FromMinutes(1));
});

var app = builder.Build();

app.UseVeffDashboard();
app.UseVeffExternalApi();

app.MapGet("/", () => "Hello World!");

app.Run();