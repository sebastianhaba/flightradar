using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlightRadar.Shared;

namespace FlightRadar.UI.ViewModels;

public partial class SidePanelViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isPanelOpen;

    [ObservableProperty]
    private AircraftData? _selectedAircraft;

    [ObservableProperty]
    private ObservableCollection<AircraftData>? _aircraft;

    public DetailViewModel Detail { get; }

    public SidePanelViewModel()
    {
        Detail = new DetailViewModel();
    }

    partial void OnSelectedAircraftChanged(AircraftData? value)
    {
        Detail.SelectedAircraft = value;
    }

    public void UpdateDetailCenter(double centerLat, double centerLon)
    {
        Detail.CenterLat = centerLat;
        Detail.CenterLon = centerLon;
    }

    [RelayCommand]
    private void TogglePanel()
    {
        IsPanelOpen = !IsPanelOpen;
    }
}
