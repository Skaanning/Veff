using JasperFx;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Veff;
using Veff.Marten;
using WebTester;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten(opt =>
{
    opt.Connection(builder.Configuration.GetConnectionString("martenConnectionString")!);
    opt.AutoCreateSchemaObjects = AutoCreate.All;
});

builder.Services.AddVeff(veffBuilder =>
{
    veffBuilder
        .AddMarten(TimeSpan.FromSeconds(5))
        .AddFeatureFlagContainersFromAssembly()
        .AddDashboardAuthorizersFromAssembly()
        .AddExternalApiAuthorizersFromAssembly();
});

// builder.Services.AddCors();

var app = builder.Build();

// app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

await app.UseVeff(s =>
{
    s.UseVeffDashboard();
    s.UseVeffExternalApi();
});

app.MapGet("/", ([FromServices]NewStuffFeatures featureFlagContainer, [FromServices] EmailFeatures ef) => 
$"""
featureFlagContainer.CanUseEmails.IsEnabled = {featureFlagContainer.CanUseEmails.IsEnabled}
EmailFeatures.SendActualEmails.EnabledFor("me") = {ef.SendActualEmails.EnabledFor("me")}
""");

app.Run();