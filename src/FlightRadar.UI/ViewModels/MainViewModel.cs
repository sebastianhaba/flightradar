using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlightRadar.UI.Resources;
using FlightRadar.UI.Services;
using FlightRadar.Shared;

namespace FlightRadar.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly RadarHubClient _hub;
    public PingService PingService { get; }

    [ObservableProperty]
    private ObservableCollection<AircraftData> _aircraft = [];

    public SidePanelViewModel SidePanel { get; }
    public HistoryViewModel History { get; }

    [ObservableProperty]
    private int _aircraftCount;

    [ObservableProperty]
    private string _connectionStatus = SR.Status_Connecting;

    [ObservableProperty]
    private string _lastUpdate = "";

    [ObservableProperty]
    private string _aircraftCountDisplay = string.Format(SR.Status_AircraftCountFormat, 0);

    [ObservableProperty]
    private string _lastUpdateDisplay = "";

    [ObservableProperty]
    private double _centerLat;

    [ObservableProperty]
    private double _centerLon;

    [ObservableProperty]
    private IBrush _statusBrush = new SolidColorBrush(Colors.Orange);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHistoryView))]
    private bool _isLiveView = true;

    public bool IsHistoryView => !IsLiveView;

    [ObservableProperty]
    private bool _isMuted = true;

    public MainViewModel(RadarHubClient hub, PingService pingService)
    {
        _hub = hub;
        PingService = pingService;
        SidePanel = new SidePanelViewModel { Aircraft = Aircraft };
        History = new HistoryViewModel();

        _hub.OnRadarUpdate += state => Dispatcher.UIThread.Post(() => OnRadarUpdate(state));
        _hub.OnConnectionStateChanged += state => Dispatcher.UIThread.Post(() => OnConnectionStateChanged(state));
    }

    [RelayCommand]
    private void ToggleMute()
    {
        IsMuted = !IsMuted;
        PingService.IsMuted = IsMuted;
        PingService.OnMuteChanged?.Invoke(IsMuted);
        PingService.RequestAudioInit?.Invoke();
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
        AircraftCountDisplay = string.Format(SR.Status_AircraftCountFormat, count);
        LastUpdate = state.Timestamp.ToLocalTime().ToString("HH:mm:ss");
        LastUpdateDisplay = string.Format(SR.Status_LastUpdateFormat, LastUpdate);
        CenterLat = state.CenterLat;
        CenterLon = state.CenterLon;

        SidePanel.UpdateDetailCenter(CenterLat, CenterLon);

        var stillSelected = fresh.FirstOrDefault(a => a.IcaoHex == selectedIcao);
        SidePanel.SelectedAircraft = stillSelected;
    }

    private void OnConnectionStateChanged(ConnectionState state)
    {
        ConnectionStatus = state switch
        {
            ConnectionState.Connected => SR.Status_Connected,
            ConnectionState.Disconnected => SR.Status_Disconnected,
            ConnectionState.Reconnecting => SR.Status_Reconnecting,
            ConnectionState.Failed => SR.Status_Failed,
            _ => SR.Status_Connecting
        };
        StatusBrush = state switch
        {
            ConnectionState.Connected => new SolidColorBrush(Colors.LimeGreen),
            ConnectionState.Disconnected or ConnectionState.Failed => new SolidColorBrush(Colors.Red),
            _ => new SolidColorBrush(Colors.Orange)
        };
    }

    [RelayCommand]
    private void SwitchToLive() => IsLiveView = true;

    [RelayCommand]
    private void SwitchToHistory() => IsLiveView = false;
}
