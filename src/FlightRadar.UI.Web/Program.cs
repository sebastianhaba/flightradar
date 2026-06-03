using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Browser;

[assembly: SupportedOSPlatform("browser")]

internal partial class Program
{
    private static async Task Main(string[] args)
    {
        var hubArg = args.FirstOrDefault(a => a.StartsWith("--hub="));
        if (hubArg != null)
        {
            var hubUrl = hubArg["--hub=".Length..];
            Environment.SetEnvironmentVariable("HUB_URL", hubUrl);
        }

        await BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<FlightRadar.UI.App>();
}
