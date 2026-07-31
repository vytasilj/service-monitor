namespace ServiceMonitor.App.Monitoring;

public static class HistoryTransitionEvaluator
{
    // We only want a new row when the state actually changes — repeatedly
    // logging "still Ok" every 30 seconds would make the history useless noise.
    public static bool ShouldRecord(HealthState? previousState, HealthState newState)
        => previousState is null || previousState.Value != newState;
}