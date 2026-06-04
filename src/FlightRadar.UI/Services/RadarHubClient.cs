using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using FlightRadar.Shared;
using FlightRadar.UI.Configuration;

namespace FlightRadar.UI.Services;

public class RadarHubClient
{
    private readonly HubConnection _connection;
    public string HubUrl { get; }

    public static Action<string>? Log { get; set; }

    public event Action<RadarState>? OnRadarUpdate;
    public event Action<string>? OnConnectionStateChanged;

    public RadarHubClient()
    {
        var baseUrl = AppOptions.BaseUrl;
        if (string.IsNullOrEmpty(baseUrl))
            baseUrl = Environment.GetEnvironmentVariable("HUB_URL") ?? "http://localhost:8080";

        HubUrl = $"{baseUrl}/hubs/radar";
        Log?.Invoke($"[RadarHub] hubUrl={HubUrl}");

        _connection = new HubConnectionBuilder()
            .WithUrl(HubUrl)
            .AddJsonProtocol(o => o.PayloadSerializerOptions.TypeInfoResolver = AppJsonContext.Default)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<RadarState>("RadarUpdate", state =>
        {
            Log?.Invoke($"[RadarHub] Received {state.TotalAircraft} aircraft");
            OnRadarUpdate?.Invoke(state);
        });

        _connection.Reconnecting += _ =>
        {
            var msg = $"Reconnecting to {HubUrl}...";
            Log?.Invoke(msg);
            OnConnectionStateChanged?.Invoke(msg);
            return Task.CompletedTask;
        };

        _connection.Reconnected += _ =>
        {
            var msg = $"Connected {HubUrl}";
            Log?.Invoke(msg);
            OnConnectionStateChanged?.Invoke(msg);
            return Task.CompletedTask;
        };

        _connection.Closed += ex =>
        {
            var msg = ex is null ? "Disconnected" : $"Disconnected: {ex.Message}";
            Log?.Invoke(msg);
            OnConnectionStateChanged?.Invoke(msg);
            return Task.CompletedTask;
        };
    }

    public async Task StartAsync()
    {
        try
        {
            var msg = $"Connecting to {HubUrl}...";
            Log?.Invoke(msg);
            OnConnectionStateChanged?.Invoke(msg);
            await _connection.StartAsync();
            var ok = $"Connected {HubUrl}";
            Log?.Invoke(ok);
            OnConnectionStateChanged?.Invoke(ok);
        }
        catch (Exception ex)
        {
            var msg = $"Failed: {ex.Message}";
            Log?.Invoke(msg);
            OnConnectionStateChanged?.Invoke(msg);
        }
    }
}
