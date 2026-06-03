using System.Collections.Generic;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using FlightRadar.UI.Services;
using FlightRadar.Shared;

namespace FlightRadar.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly RadarHubClient _hub;

    [ObservableProperty]
    private List<AircraftData> _aircraft = [];

    [ObservableProperty]
    private int _aircraftCount;

    [ObservableProperty]
    private string _connectionStatus = "Łączenie...";

    [ObservableProperty]
    private string _lastUpdate = "";

    [ObservableProperty]
    private string _aircraftCountDisplay = "0 aircraft";

    [ObservableProperty]
    private string _lastUpdateDisplay = "";

    [ObservableProperty]
    private double _centerLat;

    [ObservableProperty]
    private double _centerLon;

    [ObservableProperty]
    private IBrush _statusBrush = new SolidColorBrush(Colors.Orange);

    public MainViewModel(RadarHubClient hub)
    {
        _hub = hub;
        _hub.OnRadarUpdate += OnRadarUpdate;
        _hub.OnConnectionStateChanged += state =>
        {
            ConnectionStatus = state;
            StatusBrush = state switch
            {
                "Connected" => new SolidColorBrush(Colors.LimeGreen),
                "Disconnected" => new SolidColorBrush(Colors.Red),
                _ => new SolidColorBrush(Colors.Orange)
            };
        };
    }

    private void OnRadarUpdate(RadarState state)
    {
        var count = state.Aircraft?.Count ?? 0;
        Aircraft = state.Aircraft ?? [];
        AircraftCount = count;
        AircraftCountDisplay = $"{count} aircraft";
        LastUpdate = state.Timestamp.ToLocalTime().ToString("HH:mm:ss");
        LastUpdateDisplay = $"Last: {LastUpdate}";
        CenterLat = state.CenterLat;
        CenterLon = state.CenterLon;
    }
}
