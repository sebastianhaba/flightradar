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
    public static Action<string>? OpenUrl { get; set; }

    public event Action<RadarState>? OnRadarUpdate;
    public event Action<ConnectionState>? OnConnectionStateChanged;

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
            Log?.Invoke($"Reconnecting to {HubUrl}...");
            OnConnectionStateChanged?.Invoke(ConnectionState.Reconnecting);
            return Task.CompletedTask;
        };

        _connection.Reconnected += _ =>
        {
            Log?.Invoke($"Connected {HubUrl}");
            OnConnectionStateChanged?.Invoke(ConnectionState.Connected);
            return Task.CompletedTask;
        };

        _connection.Closed += async ex =>
        {
            Log?.Invoke(ex is null ? "Disconnected" : $"Disconnected: {ex.Message}");
            OnConnectionStateChanged?.Invoke(ConnectionState.Disconnected);

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
                Log?.Invoke($"Reconnecting to {HubUrl}...");
                OnConnectionStateChanged?.Invoke(ConnectionState.Reconnecting);
                _connection = BuildConnection();
                WireConnection();

                await old.DisposeAsync();
                await _connection.StartAsync();

                Log?.Invoke($"Connected {HubUrl}");
                OnConnectionStateChanged?.Invoke(ConnectionState.Connected);
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
            Log?.Invoke($"Connecting to {HubUrl}...");
            OnConnectionStateChanged?.Invoke(ConnectionState.Connecting);
            await _connection.StartAsync();
            Log?.Invoke($"Connected {HubUrl}");
            OnConnectionStateChanged?.Invoke(ConnectionState.Connected);
        }
        catch (Exception ex)
        {
            Log?.Invoke($"Failed: {ex.Message}");
            OnConnectionStateChanged?.Invoke(ConnectionState.Failed);

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
