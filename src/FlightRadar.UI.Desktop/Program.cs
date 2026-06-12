using System.Globalization;
using Avalonia;
using FlightRadar.UI;
using FlightRadar.UI.Resources;

internal class Program
{
    public static void Main(string[] args)
    {
        SetCulture();
        App.PingPlayer = new FlightRadar.UI.Desktop.DesktopPingPlayer();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<FlightRadar.UI.App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }

    private static void SetCulture()
    {
        var ci = CultureInfo.CurrentCulture;
        var culture = ci.Name.StartsWith("pl")
            ? ci
            : new CultureInfo("en-US");
        SR.SetCulture(culture);
    }
}
