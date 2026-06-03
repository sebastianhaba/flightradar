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
}
