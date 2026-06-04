using System;
using CommunityToolkit.Mvvm.ComponentModel;
using FlightRadar.Shared;

namespace FlightRadar.UI.ViewModels;

public partial class DetailViewModel : ViewModelBase
{
    [ObservableProperty]
    private AircraftData? _selectedAircraft;

    [ObservableProperty]
    private double _centerLat;

    [ObservableProperty]
    private double _centerLon;

    public bool HasSelection => SelectedAircraft is not null;
    public string? DetailCallsign => SelectedAircraft?.Callsign;
    public string? DetailIcaoHex => SelectedAircraft?.IcaoHex;
    public double? DetailLatitude => SelectedAircraft?.Latitude;
    public double? DetailLongitude => SelectedAircraft?.Longitude;
    public int? DetailAltitude => SelectedAircraft?.Altitude;
    public int? DetailAltBaro => SelectedAircraft?.AltBaro;
    public double? DetailGroundSpeed => SelectedAircraft?.GroundSpeed;
    public double? DetailHeading => SelectedAircraft?.Heading;
    public string? DetailCategory => SelectedAircraft?.Category;
    public string? DetailSquawk => SelectedAircraft?.Squawk;
    public bool? DetailMlat => SelectedAircraft?.Mlat;
    public double? DetailSeenPos => SelectedAircraft?.SeenPos;
    public string? DetailFirstSeen => SelectedAircraft?.FirstSeen?.ToLocalTime().ToString("HH:mm:ss");
    public string? DetailLastSeen => SelectedAircraft?.LastSeen?.ToLocalTime().ToString("HH:mm:ss");
    public string? DetailType => SelectedAircraft?.IsHelicopter == true ? "Helicopter" : "Airplane";

    public double? DetailDistanceKm => ComputeDistance();
    public double? DetailBearing => ComputeBearing();

    partial void OnSelectedAircraftChanged(AircraftData? value)
    {
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(DetailCallsign));
        OnPropertyChanged(nameof(DetailIcaoHex));
        OnPropertyChanged(nameof(DetailLatitude));
        OnPropertyChanged(nameof(DetailLongitude));
        OnPropertyChanged(nameof(DetailAltitude));
        OnPropertyChanged(nameof(DetailAltBaro));
        OnPropertyChanged(nameof(DetailGroundSpeed));
        OnPropertyChanged(nameof(DetailHeading));
        OnPropertyChanged(nameof(DetailCategory));
        OnPropertyChanged(nameof(DetailSquawk));
        OnPropertyChanged(nameof(DetailMlat));
        OnPropertyChanged(nameof(DetailSeenPos));
        OnPropertyChanged(nameof(DetailFirstSeen));
        OnPropertyChanged(nameof(DetailLastSeen));
        OnPropertyChanged(nameof(DetailType));
        OnPropertyChanged(nameof(DetailDistanceKm));
        OnPropertyChanged(nameof(DetailBearing));
    }

    private double? ComputeDistance()
    {
        if (SelectedAircraft is null) return null;
        var dx = (SelectedAircraft.Longitude - CenterLon) * 111320 * Math.Cos(CenterLat * Math.PI / 180);
        var dy = (SelectedAircraft.Latitude - CenterLat) * 111320;
        return Math.Sqrt(dx * dx + dy * dy) / 1000;
    }

    private double? ComputeBearing()
    {
        if (SelectedAircraft is null) return null;
        var dx = (SelectedAircraft.Longitude - CenterLon) * 111320 * Math.Cos(CenterLat * Math.PI / 180);
        var dy = (SelectedAircraft.Latitude - CenterLat) * 111320;
        var bearing = Math.Atan2(dx, dy) * 180 / Math.PI;
        if (bearing < 0) bearing += 360;
        return bearing;
    }
}
