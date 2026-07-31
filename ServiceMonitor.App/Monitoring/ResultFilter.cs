namespace ServiceMonitor.App.Monitoring;

public static class ResultFilter
{
    public static bool Matches(string name, HealthState state, string? nameFilter, HealthState? stateFilter)
    {
        if (stateFilter.HasValue && state != stateFilter.Value) return false;

        if (!string.IsNullOrWhiteSpace(nameFilter) &&
            !name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)) return false;

        return true;
    }
}