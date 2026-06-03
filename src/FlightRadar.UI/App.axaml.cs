using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FlightRadar.UI.Services;
using FlightRadar.UI.ViewModels;
using FlightRadar.UI.Views;

namespace FlightRadar.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var hub = new RadarHubClient();
        var vm = new MainViewModel(hub);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Window
            {
                Title = "FlightRadar",
                Width = 800,
                Height = 800,
                Content = new MainView { DataContext = vm }
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime single)
        {
            single.MainView = new MainView { DataContext = vm };
        }

        _ = hub.StartAsync();
        base.OnFrameworkInitializationCompleted();
    }
}
