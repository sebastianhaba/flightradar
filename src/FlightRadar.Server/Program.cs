using FlightRadar.Server.Hubs;
using FlightRadar.Server.Services;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<AircraftTracker>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<AdsbPoller>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<AdsbPoller>());

var app = builder.Build();

app.UseDefaultFiles();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".wasm"] = "application/wasm";
provider.Mappings[".dat"] = "application/octet-stream";

var staticOptions = new StaticFileOptions
{
    ContentTypeProvider = provider,
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
};
app.UseStaticFiles(staticOptions);

app.UseWebSockets();
app.MapHub<RadarHub>("/hubs/radar");

app.MapFallbackToFile("index.html");

app.Run();
