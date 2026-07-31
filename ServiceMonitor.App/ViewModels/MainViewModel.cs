using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ServiceMonitor.App.Monitoring;

namespace ServiceMonitor.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<CheckResult> Results { get; }

    public MainViewModel(MonitorResultsStore store)
    {
        Results = store.Results;
    }
}