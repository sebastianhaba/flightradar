using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using FlightRadar.UI.Services;
using FlightRadar.Shared;

namespace FlightRadar.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly RadarHubClient _hub;

    public ObservableCollection<AircraftData> Aircraft { get; } = [];

    public SidePanelViewModel SidePanel { get; }

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
        SidePanel = new SidePanelViewModel { Aircraft = Aircraft };

        _hub.OnRadarUpdate += state => Dispatcher.UIThread.Post(() => OnRadarUpdate(state));

        _hub.OnConnectionStateChanged += state =>
        {
            ConnectionStatus = state;
            StatusBrush = state.StartsWith("Connected") ? new SolidColorBrush(Colors.LimeGreen)
                : state.StartsWith("Disconnected") || state.StartsWith("Failed") ? new SolidColorBrush(Colors.Red)
                : new SolidColorBrush(Colors.Orange);
        };
    }

    private void OnRadarUpdate(RadarState state)
    {
        var sorted = state.Aircraft?
            .OrderByDescending(a => a.FirstSeen ?? DateTime.MinValue)
            .ToList() ?? [];

        var selectedIcao = SidePanel.SelectedAircraft?.IcaoHex;

        Aircraft.Clear();
        foreach (var ac in sorted)
            Aircraft.Add(ac);

        var count = Aircraft.Count;
        AircraftCount = count;
        AircraftCountDisplay = $"{count} aircraft";
        LastUpdate = state.Timestamp.ToLocalTime().ToString("HH:mm:ss");
        LastUpdateDisplay = $"Last: {LastUpdate}";
        CenterLat = state.CenterLat;
        CenterLon = state.CenterLon;

        SidePanel.UpdateDetailCenter(CenterLat, CenterLon);

        var stillSelected = Aircraft.FirstOrDefault(a => a.IcaoHex == selectedIcao);
        SidePanel.SelectedAircraft = stillSelected;
    }
}
