using Microsoft.AspNetCore.Mvc;
using Veff;
using Veff.SqlServer;
using WebTester;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddVeff(veffBuilder =>
{
    var connectionString = builder.Configuration.GetConnectionString("SqlDb")!;

    veffBuilder
        .WithSqlServer(connectionString, TimeSpan.FromSeconds(30))
        .AddFeatureFlagContainersFromAssembly()
        .AddDashboardAuthorizersFromAssembly()
        .AddExternalApiAuthorizersFromAssembly();
});

var app = builder.Build();

app.UseVeff(s =>
{
    s.UseVeffDashboard();
    s.UseVeffExternalApi();
});

app.MapGet("/", ([FromServices]NewStuffFeatures featureFlagContainer, [FromServices] EmailFeatures ef) 
    => $"{nameof(featureFlagContainer.CanUseEmails)}={featureFlagContainer.CanUseEmails.IsEnabled} Hello World! EmailFeatures.SendActualEmails.enabledfor('me') = {ef.SendActualEmails.EnabledFor("me")}");

app.Run();