using Avalonia.Controls;
using FlightRadar.UI.ViewModels;

namespace FlightRadar.UI.Views;

public partial class HistoryView : UserControl
{
    public HistoryView()
    {
        InitializeComponent();
    }

    private void OnFlightSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox lb && lb.SelectedItem is HistoryFlightItem item && DataContext is HistoryViewModel vm)
            vm.SelectedFlight = item;
    }
}
