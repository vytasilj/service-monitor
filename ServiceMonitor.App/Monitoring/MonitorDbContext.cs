using Microsoft.EntityFrameworkCore;

namespace ServiceMonitor.App.Monitoring;

public class MonitorDbContext(DbContextOptions<MonitorDbContext> options) : DbContext(options)
{
    public DbSet<StatusHistoryEntry> StatusHistory => Set<StatusHistoryEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StatusHistoryEntry>()
            .Property(e => e.State)
            .HasConversion<string>();

        modelBuilder.Entity<StatusHistoryEntry>()
            .HasIndex(e => new { e.Source, e.Name, e.OccurredAtUtc });
    }
}