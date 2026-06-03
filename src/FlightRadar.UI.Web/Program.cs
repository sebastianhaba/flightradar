using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Browser;

[assembly: SupportedOSPlatform("browser")]

internal partial class Program
{
    private static async Task Main(string[] args)
    {
        var hubArg = args.FirstOrDefault(a => a.StartsWith("--hub="));
        if (hubArg is not null)
            Environment.SetEnvironmentVariable("HUB_URL", hubArg.Replace("--hub=", ""));

        await BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<FlightRadar.UI.App>();
}
