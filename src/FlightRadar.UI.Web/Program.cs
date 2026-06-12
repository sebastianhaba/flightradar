using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Browser;
using FlightRadar.UI;
using FlightRadar.UI.Configuration;
using FlightRadar.UI.Resources;
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

        SetCulture();

        App.PingPlayer = new FlightRadar.UI.Web.WebPingPlayer();
        try { App.InitialMuted = Browser.LoadMute(); } catch { }
        PingService.OnMuteChanged = Browser.SaveMute;
        PingService.RequestAudioInit = FlightRadar.UI.Web.WebPingPlayer.Browser.InitAudio;

        await BuildAvaloniaApp()
             .WithInterFont()
             .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<FlightRadar.UI.App>();

    private static void SetCulture()
    {
        var lang = Browser.GetSystemLanguage();
        Browser.ConsoleLog($"[FlightRadar] Language={lang}");
        var culture = lang.ToLower().StartsWith("pl")
            ? new CultureInfo("pl-PL")
            : new CultureInfo("en-US");
        SR.SetCulture(culture);
    }

    [SupportedOSPlatform("browser")]
    public static partial class Browser
    {
        [JSImport("getOrigin", "MyInterop")]
        public static partial string GetOrigin();

        [JSImport("consoleLog", "MyInterop")]
        public static partial void ConsoleLog(string message);

        [JSImport("openUrl", "MyInterop")]
        public static partial void OpenUrl(string url);

        [JSImport("saveMute", "MyInterop")]
        public static partial void SaveMute(bool muted);

        [JSImport("loadMute", "MyInterop")]
        public static partial bool LoadMute();

        [JSImport("getSystemLanguage", "MyInterop")]
        public static partial string GetSystemLanguage();
    }
}
