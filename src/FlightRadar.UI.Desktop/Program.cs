using Avalonia;
using FlightRadar.UI;

internal class Program
{
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
    {
        App.PingPlayer = new FlightRadar.UI.Desktop.DesktopPingPlayer();
        return AppBuilder.Configure<FlightRadar.UI.App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
