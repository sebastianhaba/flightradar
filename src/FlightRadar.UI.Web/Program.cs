using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Browser;
using FlightRadar.UI.Configuration;
using FlightRadar.UI.Services;

[assembly: SupportedOSPlatform("browser")]

internal partial class Program
{
    private static async Task Main(string[] args)
    {
        await JSHost.ImportAsync("MyInterop", "../interop.js");
        AppOptions.BaseUrl = Browser.GetOrigin();
        RadarHubClient.Log = Browser.ConsoleLog;
        RadarHubClient.OpenUrl = Browser.OpenUrl;
        Browser.ConsoleLog($"[FlightRadar] BaseUrl={AppOptions.BaseUrl}");

        await BuildAvaloniaApp()
             .WithInterFont()
             .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<FlightRadar.UI.App>();

    [SupportedOSPlatform("browser")]
    public static partial class Browser
    {
        [JSImport("getOrigin", "MyInterop")]
        public static partial string GetOrigin();

        [JSImport("consoleLog", "MyInterop")]
        public static partial void ConsoleLog(string message);

        [JSImport("openUrl", "MyInterop")]
        public static partial void OpenUrl(string url);
    }
}
