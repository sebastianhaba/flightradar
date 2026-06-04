using FlightRadar.Shared;

namespace FlightRadar.Server.Services;

public class AircraftTracker
{
    private readonly Dictionary<string, AircraftData> _aircraft = new();
    private readonly object _lock = new();
    private const int StaleTimeoutSeconds = 30;

    public List<AircraftData> Update(List<AircraftData> freshList)
    {
        var now = DateTime.UtcNow;
        var freshIcaos = new HashSet<string>();

        lock (_lock)
        {
            foreach (var ac in freshList)
            {
                freshIcaos.Add(ac.IcaoHex);

                if (_aircraft.TryGetValue(ac.IcaoHex, out var existing))
                {
                    existing.Latitude = ac.Latitude;
                    existing.Longitude = ac.Longitude;
                    existing.Heading = ac.Heading;
                    existing.Altitude = ac.Altitude;
                    existing.GroundSpeed = ac.GroundSpeed;
                    existing.Callsign = ac.Callsign;
                    existing.Category = ac.Category;
                    existing.AltBaro = ac.AltBaro;
                    existing.Squawk = ac.Squawk;
                    existing.Mlat = ac.Mlat;
                    existing.SeenPos = ac.SeenPos;
                    existing.LastSeen = now;
                    existing.IsStale = false;
                }
                else
                {
                    ac.FirstSeen = now;
                    ac.LastSeen = now;
                    ac.IsStale = false;
                    _aircraft[ac.IcaoHex] = ac;
                }
            }

            var staleThreshold = now.AddSeconds(-StaleTimeoutSeconds);
            var toRemove = new List<string>();
            var result = new List<AircraftData>();

            foreach (var kvp in _aircraft)
            {
                if (kvp.Value.LastSeen < staleThreshold)
                {
                    if (kvp.Value.IsStale)
                        toRemove.Add(kvp.Key);
                    else
                    {
                        kvp.Value.IsStale = true;
                        result.Add(kvp.Value);
                    }
                }
                else
                {
                    result.Add(kvp.Value);
                }
            }

            foreach (var icao in toRemove)
                _aircraft.Remove(icao);

            return result;
        }
    }
}
