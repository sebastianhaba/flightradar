using FlightRadar.Shared;
using LiteDB;

namespace FlightRadar.Server.Services;

public class FlightHistoryService : IDisposable
{
    private readonly ILiteDatabase _db;
    private readonly ILiteCollection<FlightRecord> _flights;
    private readonly Dictionary<string, string> _activeFlights = [];
    private readonly object _lock = new();
    private readonly ILogger<FlightHistoryService> _log;

    public FlightHistoryService(ILogger<FlightHistoryService> log)
    {
        _log = log;
        var dataDir = Environment.GetEnvironmentVariable("DATA_DIR") ?? (OperatingSystem.IsWindows() ? "data" : "/data");
        Directory.CreateDirectory(dataDir);
        BsonMapper.Global.Entity<FlightRecord>().Id(x => x.Id, false);

        _db = new LiteDatabase(Path.Combine(dataDir, "flightradar.db"));
        _flights = _db.GetCollection<FlightRecord>("flights");
        _flights.EnsureIndex(x => x.FirstSeen);
    }

    public void RecordPoll(List<AircraftData> freshAircraft)
    {
        var now = DateTime.UtcNow;

        lock (_lock)
        {
            var seen = new HashSet<string>(freshAircraft.Select(a => a.IcaoHex));

            foreach (var icao in _activeFlights.Keys.ToList())
            {
                if (!seen.Contains(icao))
                {
                    var flight = _flights.FindById(_activeFlights[icao]);
                    if (flight is not null)
                    {
                        flight.LastSeen = now;
                        _flights.Update(flight);
                    }
                    _activeFlights.Remove(icao);
                }
            }

            foreach (var ac in freshAircraft)
            {
                if (_activeFlights.TryGetValue(ac.IcaoHex, out var flightId))
                {
                    var flight = _flights.FindById(flightId);
                    if (flight is not null)
                    {
                        flight.TrackPoints.Add(new TrackPoint
                        {
                            Timestamp = now,
                            Lat = ac.Latitude,
                            Lon = ac.Longitude,
                            Heading = ac.Heading,
                            Altitude = ac.Altitude,
                            GroundSpeed = ac.GroundSpeed
                        });
                        flight.LastSeen = now;
                        _flights.Update(flight);
                    }
                }
                else
                {
                    var flight = new FlightRecord
                    {
                        Id = ObjectId.NewObjectId().ToString(),
                        IcaoHex = ac.IcaoHex,
                        Callsign = ac.Callsign,
                        Category = ac.Category,
                        TypeCode = ac.TypeCode,
                        Registration = ac.Registration,
                        Description = ac.Description,
                        OwnOp = ac.OwnOp,
                        FirstLat = ac.Latitude,
                        FirstLon = ac.Longitude,
                        FirstAltitude = ac.Altitude,
                        FirstHeading = ac.Heading,
                        FirstGroundSpeed = ac.GroundSpeed,
                        FirstSeen = now,
                        LastSeen = now,
                        TrackPoints =
                        [
                            new TrackPoint
                            {
                                Timestamp = now,
                                Lat = ac.Latitude,
                                Lon = ac.Longitude,
                                Heading = ac.Heading,
                                Altitude = ac.Altitude,
                                GroundSpeed = ac.GroundSpeed
                            }
                        ]
                    };
                    _flights.Insert(flight);
                    _activeFlights[ac.IcaoHex] = flight.Id!;
                }
            }
        }
    }

    public List<FlightRecord> GetFlightsForDate(DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        lock (_lock)
        {
            return _flights.Find(x => x.FirstSeen >= dayStart && x.FirstSeen < dayEnd)
                .OrderByDescending(x => x.FirstSeen)
                .ToList();
        }
    }

    public void Dispose()
    {
        _db?.Dispose();
    }
}
