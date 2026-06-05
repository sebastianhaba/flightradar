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

    [ObservableProperty]
    private ObservableCollection<AircraftData> _aircraft = [];

    public SidePanelViewModel SidePanel { get; }

    [ObservableProperty]
    private int _aircraftCount;

    [ObservableProperty]
    private string _connectionStatus = "Łączenie...";

    [ObservableProperty]
    private string _lastUpdate = "";

    [ObservableProperty]
    private string _aircraftCountDisplay = "Statki powietrzne: 0";

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
        _hub.OnConnectionStateChanged += state => Dispatcher.UIThread.Post(() => OnConnectionStateChanged(state));
    }

    private void OnRadarUpdate(RadarState state)
    {
        var sorted = state.Aircraft?
            .OrderByDescending(a => a.FirstSeen ?? DateTime.MinValue)
            .ToList() ?? [];

        var selectedIcao = SidePanel.SelectedAircraft?.IcaoHex;

        var fresh = new ObservableCollection<AircraftData>(sorted);
        SidePanel.Aircraft = fresh;
        Aircraft = fresh;

        var count = fresh.Count;
        AircraftCount = count;
        AircraftCountDisplay = $"Statki powietrzne: {count}";
        LastUpdate = state.Timestamp.ToLocalTime().ToString("HH:mm:ss");
        LastUpdateDisplay = $"Ostatnia: {LastUpdate}";
        CenterLat = state.CenterLat;
        CenterLon = state.CenterLon;

        SidePanel.UpdateDetailCenter(CenterLat, CenterLon);

        var stillSelected = fresh.FirstOrDefault(a => a.IcaoHex == selectedIcao);
        SidePanel.SelectedAircraft = stillSelected;
    }

    private void OnConnectionStateChanged(string state)
    {
        ConnectionStatus = state;
        StatusBrush = state.StartsWith("Connected") ? new SolidColorBrush(Colors.LimeGreen)
            : state.StartsWith("Disconnected") || state.StartsWith("Failed") ? new SolidColorBrush(Colors.Red)
            : new SolidColorBrush(Colors.Orange);
    }
}
