using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Veff;
using Veff.Sqlite;
using WebTester;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddVeff(veffBuilder =>
{
    veffBuilder
        .AddSqlite("Data Source=veff1.db;", TimeSpan.FromSeconds(5))
        .AddFeatureFlagContainersFromAssembly()
        .AddDashboardAuthorizersFromAssembly()
        .AddExternalApiAuthorizersFromAssembly();
});

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

await app.UseVeff(s =>
{
    s.UseVeffDashboard();
    s.UseVeffExternalApi();
});

app.MapGet("/", ([FromServices]EmailFeatures emailFeatures, [FromServices] NewStuffFeatures newStuffFeatures) => 
$"""

{emailFeatures.SendSpamMails.Name} = {emailFeatures.SendSpamMails.IsEnabled}

{newStuffFeatures.SendActualEmails.Name}.IsEnabledFor("Bobby") = {newStuffFeatures.SendActualEmails.EnabledFor("Bobby")}

{newStuffFeatures.Hello.Name}.IsEnabled = {newStuffFeatures.Hello.IsEnabled}

{newStuffFeatures.SomeDateFeatureFlag.Name}.IsEnabledNow = {newStuffFeatures.SomeDateFeatureFlag.IsEnabledNow()}

""");

app.MapGet("/snapshot", ([FromServices] EmailFeatures emailFeatures) =>
{
    var snapshot = emailFeatures.ToSnapshot<EmailFeatureSnapshot>();
    return $"""
           {JsonSerializer.Serialize(snapshot, new JsonSerializerOptions {WriteIndented = true})}
           
           snapshot.EndingFlag.EnabledFor("Bob") = {snapshot.EndingFlag.EnabledFor("Bob")}
           snapshot.EndingFlag.EnabledFor("obby") = {snapshot.EndingFlag.EnabledFor("obby")}
           snapshot.SendSpamMails.IsEnabled = {snapshot.SendSpamMails.IsEnabled}
           snapshot.SomeDateFeatureFlag.IsEnabledNow() = {snapshot.SomeDateFeatureFlag.IsEnabledNow()}
           
           """;
});

app.Run();