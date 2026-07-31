using CommunityToolkit.Mvvm.ComponentModel;

namespace ServiceMonitor.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _greeting = "Service Monitor";
}