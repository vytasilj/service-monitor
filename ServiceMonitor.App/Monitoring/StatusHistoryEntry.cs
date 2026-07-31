namespace ServiceMonitor.App.Monitoring;

public class StatusHistoryEntry
{
    public int Id { get; set; }
    public required string Source { get; set; }
    public required string Name { get; set; }
    public HealthState State { get; set; }
    public string? Detail { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}