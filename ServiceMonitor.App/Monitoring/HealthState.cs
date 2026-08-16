namespace ServiceMonitor.App.Monitoring;

public enum HealthState
{
    Unknown,
    Ok,
    Warning,
    Error
}

public record CheckResult(string Source, string Name, HealthState State, string Detail, DateTime CheckedAtUtc)
{
    public DateTime CheckedAtLocal => CheckedAtUtc.ToLocalTime();
}