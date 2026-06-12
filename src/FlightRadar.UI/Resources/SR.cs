using System.Globalization;
using System.Resources;

namespace FlightRadar.UI.Resources;

public static class SR
{
    private static readonly ResourceManager _rm = new("FlightRadar.UI.Resources.Resources", typeof(SR).Assembly);
    private static bool _usePl;

    public static void SetCulture(CultureInfo culture)
    {
        _usePl = culture.Name.StartsWith("pl");
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    private static string GetString(string key, string fallback)
    {
        if (_usePl)
        {
            var pl = _rm.GetString("PL_" + key);
            if (pl is not null) return pl;
        }
        return _rm.GetString(key) ?? fallback;
    }

    public static string MainView_Live => GetString(nameof(MainView_Live), "Live");
    public static string MainView_History => GetString(nameof(MainView_History), "History");
    public static string History_Load => GetString(nameof(History_Load), "Load");
    public static string Panel_AircraftList => GetString(nameof(Panel_AircraftList), "Aircraft List");
    public static string TableHeader_Time => GetString(nameof(TableHeader_Time), "Time");
    public static string TableHeader_Callsign => GetString(nameof(TableHeader_Callsign), "Callsign");
    public static string TableHeader_Alt => GetString(nameof(TableHeader_Alt), "Alt");
    public static string TableHeader_Heading => GetString(nameof(TableHeader_Heading), "Hdg");
    public static string Detail_Placeholder => GetString(nameof(Detail_Placeholder), "Select an aircraft in the table to see details");
    public static string DetailLabel_Callsign => GetString(nameof(DetailLabel_Callsign), "Callsign");
    public static string DetailLabel_Icao => GetString(nameof(DetailLabel_Icao), "ICAO");
    public static string DetailLabel_Registration => GetString(nameof(DetailLabel_Registration), "Registration");
    public static string DetailLabel_Operator => GetString(nameof(DetailLabel_Operator), "Operator");
    public static string DetailLabel_Type => GetString(nameof(DetailLabel_Type), "Type");
    public static string DetailLabel_Code => GetString(nameof(DetailLabel_Code), "Code");
    public static string DetailLabel_Description => GetString(nameof(DetailLabel_Description), "Description");
    public static string Detail_Wikipedia => GetString(nameof(Detail_Wikipedia), "Wikipedia");
    public static string DetailLabel_Latitude => GetString(nameof(DetailLabel_Latitude), "Lat.");
    public static string DetailLabel_Longitude => GetString(nameof(DetailLabel_Longitude), "Lon.");
    public static string DetailLabel_Distance => GetString(nameof(DetailLabel_Distance), "Distance");
    public static string DetailLabel_Bearing => GetString(nameof(DetailLabel_Bearing), "Bearing");
    public static string DetailLabel_AltGeo => GetString(nameof(DetailLabel_AltGeo), "Alt. geo");
    public static string DetailLabel_AltBaro => GetString(nameof(DetailLabel_AltBaro), "Alt. baro");
    public static string DetailLabel_Speed => GetString(nameof(DetailLabel_Speed), "Speed");
    public static string DetailLabel_Heading => GetString(nameof(DetailLabel_Heading), "Heading");
    public static string DetailLabel_Squawk => GetString(nameof(DetailLabel_Squawk), "Squawk");
    public static string DetailLabel_FirstSeen => GetString(nameof(DetailLabel_FirstSeen), "First seen");
    public static string DetailLabel_LastSeen => GetString(nameof(DetailLabel_LastSeen), "Last seen");
    public static string Detail_Helicopter => GetString(nameof(Detail_Helicopter), "Helicopter");
    public static string Detail_Airplane => GetString(nameof(Detail_Airplane), "Airplane");
    public static string Status_Connecting => GetString(nameof(Status_Connecting), "Connecting...");
    public static string Status_AircraftCountFormat => GetString(nameof(Status_AircraftCountFormat), "Aircraft: {0}");
    public static string Status_LastUpdateFormat => GetString(nameof(Status_LastUpdateFormat), "Updated: {0}");
    public static string History_Loading => GetString(nameof(History_Loading), "Loading...");
    public static string History_FlightsFoundFormat => GetString(nameof(History_FlightsFoundFormat), "Found {0} flights");
    public static string History_ErrorFormat => GetString(nameof(History_ErrorFormat), "Error: {0}");
    public static string Status_Connected => GetString(nameof(Status_Connected), "Connected");
    public static string Status_Disconnected => GetString(nameof(Status_Disconnected), "Disconnected");
    public static string Status_Reconnecting => GetString(nameof(Status_Reconnecting), "Reconnecting...");
    public static string Status_Failed => GetString(nameof(Status_Failed), "Failed");
}
