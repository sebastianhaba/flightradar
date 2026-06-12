using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlightRadar.Shared;
using FlightRadar.UI.Configuration;
using FlightRadar.UI.Resources;

namespace FlightRadar.UI.ViewModels;

public partial class HistoryViewModel : ViewModelBase
{
    [ObservableProperty]
    private DateTimeOffset? _selectedDate = DateTimeOffset.Now;

    [ObservableProperty]
    private string _statusText = "";

    [ObservableProperty]
    private ObservableCollection<HourBlock> _hourBlocks = [];

    [ObservableProperty]
    private HistoryFlightItem? _selectedFlight;

    public DetailViewModel Detail { get; } = new();

    public HistoryViewModel()
    {
        var lat = double.TryParse(Environment.GetEnvironmentVariable("RADAR_LAT"), out var parsedLat) ? parsedLat : 52.2297;
        var lon = double.TryParse(Environment.GetEnvironmentVariable("RADAR_LON"), out var parsedLon) ? parsedLon : 21.0122;
        Detail.CenterLat = lat;
        Detail.CenterLon = lon;
    }

    [RelayCommand]
    private async Task Load()
    {
        if (SelectedDate is null) return;
        var date = SelectedDate.Value.DateTime.Date;
        StatusText = SR.History_Loading;

        try
        {
            var baseUrl = AppOptions.BaseUrl ?? Environment.GetEnvironmentVariable("HUB_URL") ?? "http://localhost:8080";
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var url = $"{baseUrl}/api/history?date={date:yyyy-MM-dd}";
            var flights = await http.GetFromJsonAsync<List<FlightRecord>>(url);

            var grouped = flights?
                .GroupBy(f => f.FirstSeen.ToLocalTime().Hour)
                .OrderByDescending(g => g.Key)
                .Select(g => new HourBlock
                {
                    Hour = g.Key,
                    Flights = new ObservableCollection<HistoryFlightItem>(
                        g.Select(f => new HistoryFlightItem { Record = f }))
                })
                .ToList() ?? [];

            HourBlocks = new ObservableCollection<HourBlock>(grouped);
            StatusText = string.Format(SR.History_FlightsFoundFormat, flights?.Count ?? 0);
        }
        catch (Exception ex)
        {
            StatusText = string.Format(SR.History_ErrorFormat, ex.Message);
        }
    }

    partial void OnSelectedFlightChanged(HistoryFlightItem? value)
    {
        if (value is not null)
            Detail.LoadFromHistory(value.Record);
    }
}

public class HourBlock
{
    public int Hour { get; set; }
    public string Display => $"{Hour:D2}:00 - {Hour:D2}:59  ({Flights.Count})";
    public bool IsExpanded => false;
    public ObservableCollection<HistoryFlightItem> Flights { get; set; } = [];
}

public class HistoryFlightItem
{
    public FlightRecord Record { get; set; } = new();

    public string? Callsign => Record.Callsign;
    public int Altitude => Record.FirstAltitude;
    public double Heading => Record.FirstHeading;
    public string FirstSeenTime => Record.FirstSeen.ToLocalTime().ToString("HH:mm");
    public string CategoryIcon => Record.Category == "A7" ? "H" : "S";
    public string? TypeCode => Record.TypeCode;
    public string? Description => Record.Description;
}
