using Microsoft.EntityFrameworkCore;

namespace ServiceMonitor.App.Monitoring;

public class HistoryService(IDbContextFactory<MonitorDbContext> dbContextFactory)
{
    public async Task RecordAsync(List<CheckResult> results, CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        foreach (var result in results)
        {
            var lastEntry = await db.StatusHistory
                .Where(h => h.Source == result.Source && h.Name == result.Name)
                .OrderByDescending(h => h.OccurredAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (!HistoryTransitionEvaluator.ShouldRecord(lastEntry?.State, result.State))
            {
                continue;
            }

            db.StatusHistory.Add(new StatusHistoryEntry
            {
                Source = result.Source,
                Name = result.Name,
                State = result.State,
                Detail = result.Detail,
                OccurredAtUtc = result.CheckedAtUtc
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<StatusHistoryEntry>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.StatusHistory
            .OrderByDescending(h => h.OccurredAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}