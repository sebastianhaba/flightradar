namespace FlightRadar.Shared;

public class AircraftData
{
    public string IcaoHex { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Heading { get; set; }
    public int Altitude { get; set; }
    public double GroundSpeed { get; set; }
    public string? Callsign { get; set; }
    public string? Category { get; set; }
    public bool IsHelicopter => Category == "A7";
    public int? AltBaro { get; set; }
    public string? Squawk { get; set; }
    public bool? Mlat { get; set; }
    public double? SeenPos { get; set; }
    public DateTime? FirstSeen { get; set; }
    public DateTime? LastSeen { get; set; }
    public bool IsStale { get; set; }

    public string? TypeCode { get; set; }
    public string? Description { get; set; }
    public string? Registration { get; set; }
    public string? OwnOp { get; set; }

    public DateTime? FirstSeenLocal => FirstSeen?.ToLocalTime();
    public DateTime? LastSeenLocal => LastSeen?.ToLocalTime();
}
