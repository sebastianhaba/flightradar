using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Browser;
using System.Runtime.InteropServices.JavaScript;
using FlightRadar.UI.Configuration;


[assembly: SupportedOSPlatform("browser")]

internal partial class Program
{
    private static async Task Main(string[] args)
    {
        await JSHost.ImportAsync("MyInterop", "../interop.js");
        AppOptions.BaseUrl = Browser.GetOrigin();

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
        
        [JSImport("showAlert", "MyInterop")]
        public static partial void ShowAlert(string message);
    }
}
