using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using FlightRadar.Shared;

namespace FlightRadar.UI.Services;

public class RadarHubClient
{
    private readonly HubConnection _connection;

    public event Action<RadarState>? OnRadarUpdate;
    public event Action<string>? OnConnectionStateChanged;

    public RadarHubClient()
    {
        var hubUrl = Environment.GetEnvironmentVariable("HUB_URL") ?? "http://localhost:8080/hubs/radar";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .AddJsonProtocol(o => o.PayloadSerializerOptions.TypeInfoResolver = AppJsonContext.Default)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<RadarState>("RadarUpdate", state =>
            OnRadarUpdate?.Invoke(state));

        _connection.Reconnecting += _ =>
        {
            OnConnectionStateChanged?.Invoke("Reconnecting...");
            return Task.CompletedTask;
        };

        _connection.Reconnected += _ =>
        {
            OnConnectionStateChanged?.Invoke("Connected");
            return Task.CompletedTask;
        };

        _connection.Closed += _ =>
        {
            OnConnectionStateChanged?.Invoke("Disconnected");
            return Task.CompletedTask;
        };
    }

    public async Task StartAsync()
    {
        try
        {
            await _connection.StartAsync();
            OnConnectionStateChanged?.Invoke("Connected");
        }
        catch
        {
            OnConnectionStateChanged?.Invoke("Connection failed");
        }
    }
}
