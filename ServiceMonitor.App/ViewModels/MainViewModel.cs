using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceMonitor.App.Monitoring;

namespace ServiceMonitor.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly HistoryService _historyService;
    private readonly ICollectionView _resultsView;
    private readonly ICollectionView _historyView;

    public ObservableCollection<CheckResult> Results { get; }
    public ObservableCollection<StatusHistoryEntry> History { get; } = [];

    public ICollectionView ResultsView => _resultsView;
    public ICollectionView HistoryView => _historyView;

    public HealthState[] StateFilterOptions { get; } =
        [HealthState.Ok, HealthState.Warning, HealthState.Error, HealthState.Unknown];

    [ObservableProperty]
    private string _nameFilter = "";

    [ObservableProperty]
    private HealthState? _selectedStateFilter;

    public MainViewModel(MonitorResultsStore store, HistoryService historyService)
    {
        _historyService = historyService;
        Results = store.Results;
        store.ResultsUpdated += OnResultsUpdated;

        _resultsView = CollectionViewSource.GetDefaultView(Results);
        _resultsView.Filter = o => o is CheckResult r && ResultFilter.Matches(r.Name, r.State, NameFilter, SelectedStateFilter);

        _historyView = CollectionViewSource.GetDefaultView(History);
        _historyView.Filter = o => o is StatusHistoryEntry h && ResultFilter.Matches(h.Name, h.State, NameFilter, SelectedStateFilter);

        _ = RefreshHistoryAsync();
    }

    partial void OnNameFilterChanged(string value)
    {
        _resultsView.Refresh();
        _historyView.Refresh();
    }

    partial void OnSelectedStateFilterChanged(HealthState? value)
    {
        _resultsView.Refresh();
        _historyView.Refresh();
    }

    [RelayCommand]
    private void ClearFilters()
    {
        NameFilter = "";
        SelectedStateFilter = null;
    }

    private void OnResultsUpdated() => _ = RefreshHistoryAsync();

    private async Task RefreshHistoryAsync()
    {
        var entries = await _historyService.GetRecentAsync(200);

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            History.Clear();
            foreach (var entry in entries) History.Add(entry);
            _historyView.Refresh();
        });
    }
}