using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ServiceMonitor.App.Monitoring;

namespace ServiceMonitor.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly HistoryService _historyService;

    public ObservableCollection<CheckResult> Results { get; }
    public ObservableCollection<StatusHistoryEntry> History { get; } = [];

    public MainViewModel(MonitorResultsStore store, HistoryService historyService)
    {
        _historyService = historyService;
        Results = store.Results;
        store.ResultsUpdated += OnResultsUpdated;

        _ = RefreshHistoryAsync();
    }

    private void OnResultsUpdated() => _ = RefreshHistoryAsync();

    private async Task RefreshHistoryAsync()
    {
        var entries = await _historyService.GetRecentAsync(200);

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            History.Clear();
            foreach (var entry in entries) History.Add(entry);
        });
    }
}