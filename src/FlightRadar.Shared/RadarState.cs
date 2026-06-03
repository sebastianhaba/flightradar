namespace FlightRadar.Shared;

public class RadarState
{
    public List<AircraftData>? Aircraft { get; set; }
    public DateTime Timestamp { get; set; }
    public int TotalAircraft { get; set; }
    public double CenterLat { get; set; }
    public double CenterLon { get; set; }
}
