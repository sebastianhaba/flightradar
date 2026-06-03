using Avalonia;

internal class Program
{
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<FlightRadar.UI.App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
