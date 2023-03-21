using Microsoft.AspNetCore.Mvc;
using Veff;
using Veff.Sqlite;

using WebTester;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddVeff(veffBuilder =>
{
    var connectionString = builder.Configuration.GetConnectionString("sqliteConnection")!;

    veffBuilder
        .WithSqlite(connectionString, TimeSpan.FromSeconds(1))
        .AddFeatureFlagContainersFromAssembly()
        .AddDashboardAuthorizersFromAssembly()
        .AddExternalApiAuthorizersFromAssembly();
});

// builder.Services.AddCors();

var app = builder.Build();

// app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseVeff(s =>
{
    s.UseVeffDashboard();
    s.UseVeffExternalApi();
});

app.MapGet("/", ([FromServices]NewStuffFeatures featureFlagContainer, [FromServices] EmailFeatures ef) 
    => $"{featureFlagContainer.CanUseEmails.IsEnabled}\nEmailFeatures.SendActualEmails.enabledfor('me') = {ef.SendActualEmails.EnabledFor("me")}");

app.Run();