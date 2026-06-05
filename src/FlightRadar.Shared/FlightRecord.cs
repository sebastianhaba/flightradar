namespace FlightRadar.Shared;

public class FlightRecord
{
    public string Id { get; set; } = "";
    public string IcaoHex { get; set; } = "";
    public string? Callsign { get; set; }
    public string? Category { get; set; }
    public string? TypeCode { get; set; }
    public string? Registration { get; set; }
    public string? Description { get; set; }
    public string? OwnOp { get; set; }

    public double FirstLat { get; set; }
    public double FirstLon { get; set; }
    public int FirstAltitude { get; set; }
    public double FirstHeading { get; set; }
    public double FirstGroundSpeed { get; set; }
    public int? FirstAltBaro { get; set; }

    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }

    public List<TrackPoint> TrackPoints { get; set; } = [];
}

public class TrackPoint
{
    public DateTime Timestamp { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    public double Heading { get; set; }
    public int Altitude { get; set; }
    public double GroundSpeed { get; set; }
}
