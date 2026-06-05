using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FlightRadar.Shared;
using Microsoft.AspNetCore.SignalR;
using FlightRadar.Server.Hubs;

namespace FlightRadar.Server.Services;

public class AdsbPoller : BackgroundService
{
    private readonly IHubContext<RadarHub> _hub;
    private readonly ILogger<AdsbPoller> _log;
    private readonly IHttpClientFactory _httpFactory;
    private readonly AircraftTracker _tracker;

    private volatile RadarState? _latestState;
    public RadarState? LatestState => _latestState;

    private static readonly string AdsbBaseUrl = Environment.GetEnvironmentVariable("ADSB_API_BASE_URL")
        ?? "https://opendata.adsb.fi/api/v3";

    private static readonly double RadarLat = double.Parse(
        Environment.GetEnvironmentVariable("RADAR_LAT") ?? "52.2297", CultureInfo.InvariantCulture);

    private static readonly double RadarLon = double.Parse(
        Environment.GetEnvironmentVariable("RADAR_LON") ?? "21.0122", CultureInfo.InvariantCulture);

    private static readonly int PollInterval = int.Parse(
        Environment.GetEnvironmentVariable("POLL_INTERVAL_SECONDS") ?? "5");

    private static readonly int RangeKm = int.Parse(
        Environment.GetEnvironmentVariable("RADAR_RANGE_KM") ?? "25");

    public AdsbPoller(IHubContext<RadarHub> hub, ILogger<AdsbPoller> log, IHttpClientFactory httpFactory, AircraftTracker tracker)
    {
        _hub = hub;
        _log = log;
        _httpFactory = httpFactory;
        _tracker = tracker;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var fetchDistNm = RangeKm * 1.33 * 0.54;

        _log.LogInformation("Polling {Url} every {Interval}s", AdsbBaseUrl, PollInterval);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var url = FormattableString.Invariant(
                    $"{AdsbBaseUrl}/lat/{RadarLat}/lon/{RadarLon}/dist/{fetchDistNm:F1}");
                using var http = _httpFactory.CreateClient();
                var resp = await http.GetFromJsonAsync<AdsbApiResponse>(url, ct);

                var freshAircraft = resp?.Aircraft
                    ?.Where(a => a.Lat is not null && a.Lon is not null
                        && a.AltGeom is not null && a.Track is not null)
                    .Select(a => new AircraftData
                    {
                        IcaoHex = a.Hex ?? "?",
                        Latitude = a.Lat!.Value,
                        Longitude = a.Lon!.Value,
                        Heading = a.Track!.Value,
                        Altitude = (int)a.AltGeom!.Value,
                        GroundSpeed = a.Gs ?? 0,
                        Callsign = string.IsNullOrWhiteSpace(a.Flight) ? null : a.Flight.Trim(),
                        Category = a.Category,
                        AltBaro = a.AltBaro is JsonElement altBaro
                            ? altBaro.ValueKind switch
                            {
                                JsonValueKind.Number => (int?)altBaro.GetDouble(),
                                JsonValueKind.String when int.TryParse(altBaro.GetString(), out var b) => b,
                                _ => null
                            }
                            : null,
                        Squawk = a.Squawk,
                        Mlat = a.Mlat?.Length > 0,
                        SeenPos = a.SeenPos
                    })
                    .ToList() ?? [];

                var trackedAircraft = _tracker.Update(freshAircraft);

                var state = new RadarState
                {
                    Aircraft = trackedAircraft,
                    Timestamp = DateTime.UtcNow,
                    TotalAircraft = trackedAircraft.Count,
                    CenterLat = RadarLat,
                    CenterLon = RadarLon
                };

                _latestState = state;

                await _hub.Clients.All.SendAsync("RadarUpdate", state, ct);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "ADS-B poll failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(PollInterval), ct);
        }
    }

    private record AdsbApiAircraft(
        [property: JsonPropertyName("hex")] string? Hex,
        [property: JsonPropertyName("flight")] string? Flight,
        [property: JsonPropertyName("lat")] double? Lat,
        [property: JsonPropertyName("lon")] double? Lon,
        [property: JsonPropertyName("track")] double? Track,
        [property: JsonPropertyName("alt_geom")] double? AltGeom,
        [property: JsonPropertyName("gs")] double? Gs,
        [property: JsonPropertyName("category")] string? Category,
        [property: JsonPropertyName("alt_baro")] JsonElement? AltBaro,
        [property: JsonPropertyName("squawk")] string? Squawk,
        [property: JsonPropertyName("mlat")] string[]? Mlat,
        [property: JsonPropertyName("seen_pos")] double? SeenPos);

    private record AdsbApiResponse(
        [property: JsonPropertyName("ac")]
        List<AdsbApiAircraft>? Aircraft);
}
