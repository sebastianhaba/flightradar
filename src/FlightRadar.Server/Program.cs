using FlightRadar.Server.Hubs;
using FlightRadar.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddHttpClient<AdsbPoller>();
builder.Services.AddSingleton<AdsbPoller>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<AdsbPoller>());

var app = builder.Build();

app.UseDefaultFiles();

var staticOptions = new StaticFileOptions
{
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
};
app.UseStaticFiles(staticOptions);

app.UseWebSockets();
app.MapHub<RadarHub>("/hubs/radar");

app.MapFallbackToFile("index.html");

app.Run();
