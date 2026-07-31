using System.Collections.ObjectModel;

namespace ServiceMonitor.App.Monitoring;

public class MonitorResultsStore
{
    public ObservableCollection<CheckResult> Results { get; } = [];
    public event Action<HealthState>? OverallStateChanged;

    private HealthState _overallState = HealthState.Unknown;
    public HealthState OverallState
    {
        get => _overallState;
        private set
        {
            if (_overallState == value) return;
            _overallState = value;
            OverallStateChanged?.Invoke(value);
        }
    }

    public void UpdateResults(List<CheckResult> results)
    {
        // Results is bound to WPF UI, so mutations must happen on the UI thread —
        // this method may be called from the background polling loop's thread.
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            Results.Clear();
            foreach (var r in results) Results.Add(r);
        });

        // HealthState is declared Unknown < Ok < Warning < Error, so Max() naturally
        // picks the worst state across all results.
        OverallState = results.Count == 0 ? HealthState.Unknown : results.Max(r => r.State);
    }
}