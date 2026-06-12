using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlightRadar.Shared;
using FlightRadar.UI.Resources;
using FlightRadar.UI.Services;

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
    public double? DetailGroundSpeedKmh => SelectedAircraft?.GroundSpeed * 1.852;
    public double? DetailHeading => SelectedAircraft?.Heading;
    public string? DetailCategory => SelectedAircraft?.Category;
    public string? DetailSquawk => SelectedAircraft?.Squawk;
    public bool? DetailMlat => SelectedAircraft?.Mlat;
    public double? DetailSeenPos => SelectedAircraft?.SeenPos;
    public string? DetailFirstSeen => SelectedAircraft?.FirstSeenLocal?.ToString("HH:mm:ss");
    public string? DetailLastSeen => SelectedAircraft?.LastSeenLocal?.ToString("HH:mm:ss");
    public string? DetailType => SelectedAircraft?.Category == "A7" ? SR.Detail_Helicopter : SR.Detail_Airplane;
    public string? DetailTypeCode => SelectedAircraft?.TypeCode;
    public string? DetailDescription => SelectedAircraft?.Description;
    public string? DetailRegistration => SelectedAircraft?.Registration;
    public string? DetailOwnOp => SelectedAircraft?.OwnOp;

    public bool HasWiki => !string.IsNullOrWhiteSpace(SelectedAircraft?.TypeCode);
    public string? DetailWikiUrl => HasWiki
        ? $"https://en.wikipedia.org/w/index.php?search={SelectedAircraft!.TypeCode}"
        : null;

    public double? DetailDistanceKm => ComputeDb().DistanceKm;
    public double? DetailBearing => ComputeDb().Bearing;

    public void LoadFromHistory(FlightRecord record)
    {
        SelectedAircraft = new AircraftData
        {
            IcaoHex = record.IcaoHex,
            Callsign = record.Callsign,
            Category = record.Category,
            TypeCode = record.TypeCode,
            Registration = record.Registration,
            Description = record.Description,
            OwnOp = record.OwnOp,
            Latitude = record.FirstLat,
            Longitude = record.FirstLon,
            Altitude = record.FirstAltitude,
            AltBaro = record.FirstAltBaro,
            Heading = record.FirstHeading,
            GroundSpeed = record.FirstGroundSpeed,
            FirstSeen = record.FirstSeen,
            LastSeen = record.LastSeen
        };
    }

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
        OnPropertyChanged(nameof(DetailGroundSpeedKmh));
        OnPropertyChanged(nameof(DetailHeading));
        OnPropertyChanged(nameof(DetailCategory));
        OnPropertyChanged(nameof(DetailSquawk));
        OnPropertyChanged(nameof(DetailMlat));
        OnPropertyChanged(nameof(DetailSeenPos));
        OnPropertyChanged(nameof(DetailFirstSeen));
        OnPropertyChanged(nameof(DetailLastSeen));
        OnPropertyChanged(nameof(DetailType));
        OnPropertyChanged(nameof(DetailTypeCode));
        OnPropertyChanged(nameof(DetailDescription));
        OnPropertyChanged(nameof(DetailRegistration));
        OnPropertyChanged(nameof(DetailOwnOp));
        OnPropertyChanged(nameof(HasWiki));
        OnPropertyChanged(nameof(DetailWikiUrl));
        OnPropertyChanged(nameof(DetailDistanceKm));
        OnPropertyChanged(nameof(DetailBearing));
    }

    [RelayCommand]
    private void OpenWiki()
    {
        if (DetailWikiUrl is not { } url) return;

        if (RadarHubClient.OpenUrl is not null)
            RadarHubClient.OpenUrl(url);
        else
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { }
    }

    private (double? DistanceKm, double? Bearing) ComputeDb()
    {
        if (SelectedAircraft is null) return (null, null);
        var (dist, bearing) = GeoMath.DistanceAndBearing(CenterLat, CenterLon, SelectedAircraft.Latitude, SelectedAircraft.Longitude);
        return (dist, bearing);
    }
}
