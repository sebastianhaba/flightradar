using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using FlightRadar.Shared;
using FlightRadar.UI.Configuration;

namespace FlightRadar.UI.Services;

public class RadarHubClient
{
    private HubConnection _connection;
    private bool _disposed;
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

        _connection = BuildConnection();
        WireConnection();
    }

    private HubConnection BuildConnection()
    {
        return new HubConnectionBuilder()
            .WithUrl(HubUrl)
            .AddJsonProtocol(o => o.PayloadSerializerOptions.TypeInfoResolver = AppJsonContext.Default)
            .WithAutomaticReconnect()
            .Build();
    }

    private void WireConnection()
    {
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

        _connection.Closed += async ex =>
        {
            var msg = ex is null ? "Disconnected" : $"Disconnected: {ex.Message}";
            Log?.Invoke(msg);
            OnConnectionStateChanged?.Invoke(msg);

            if (_disposed) return;

            await AttemptReconnect();
        };
    }

    private async Task AttemptReconnect()
    {
        while (!_disposed)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5));

                if (_disposed) return;

                var old = _connection;
                Log?.Invoke($"[RadarHub] Attempting reconnect to {HubUrl}");
                _connection = BuildConnection();
                WireConnection();

                await old.DisposeAsync();
                await _connection.StartAsync();

                var ok = $"Connected {HubUrl}";
                Log?.Invoke(ok);
                OnConnectionStateChanged?.Invoke(ok);
                return;
            }
            catch (Exception ex)
            {
                Log?.Invoke($"[RadarHub] Reconnect failed: {ex.Message}");
            }
        }
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

            if (!_disposed)
                _ = AttemptReconnect();
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _ = _connection.DisposeAsync();
    }
}
