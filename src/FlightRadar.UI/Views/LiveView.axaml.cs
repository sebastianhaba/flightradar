using Avalonia.Controls;
using FlightRadar.UI.ViewModels;

namespace FlightRadar.UI.Views;

public partial class LiveView : UserControl
{
    public LiveView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;

        var radar = this.FindControl<RadarCanvas>("RadarCanvas");
        if (radar != null)
            radar.PingRequested += vm.PingService.RequestPing;
    }
}
