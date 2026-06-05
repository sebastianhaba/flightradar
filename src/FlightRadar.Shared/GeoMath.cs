using System;

namespace FlightRadar.Shared;

public static class GeoMath
{
    public static (double DistanceKm, double Bearing) DistanceAndBearing(double fromLat, double fromLon, double toLat, double toLon)
    {
        var dx = (toLon - fromLon) * 111320.0 * Math.Cos(fromLat * Math.PI / 180);
        var dy = (toLat - fromLat) * 111320.0;
        var distKm = Math.Sqrt(dx * dx + dy * dy) / 1000;
        var bearingDeg = Math.Atan2(dx, dy) * 180 / Math.PI;
        if (bearingDeg < 0) bearingDeg += 360;
        return (distKm, bearingDeg);
    }
}
