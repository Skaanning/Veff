using JasperFx;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Veff;
using Veff.Sqlite;
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
        // .AddMarten(TimeSpan.FromSeconds(5))
        .AddSqlite("Data Source=veff1.db;", TimeSpan.FromSeconds(5))
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

app.MapGet("/", ([FromServices]EmailFeatures emailFeatures, [FromServices] EmailFeatures ef, [FromServices] NewStuffFeatures newStuffFeatures) => 
$"""
{emailFeatures.SendSpamMails.Name} = {emailFeatures.SendSpamMails.IsEnabled}
{emailFeatures.SendActualEmails.Name}.IsEnabledFor("Bobby") = {emailFeatures.SendActualEmails.EnabledFor("Bobby")}
{newStuffFeatures.Hello.Name}.IsEnabled = {newStuffFeatures.Hello.IsEnabled}
""");

app.Run();