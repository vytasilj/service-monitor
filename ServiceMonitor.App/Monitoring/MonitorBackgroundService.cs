using Microsoft.Extensions.Hosting;
using ServiceMonitor.App.Configuration;

namespace ServiceMonitor.App.Monitoring;

public class MonitorBackgroundService(
    MonitorConfig config,
    KubernetesStatusChecker k8sChecker,
    HttpEndpointChecker httpChecker,
    MonitorResultsStore store,
    HistoryService historyService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(5, config.PollIntervalSeconds));
        using var timer = new PeriodicTimer(interval);

        do
        {
            await RunOnce(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunOnce(CancellationToken cancellationToken)
    {
        var results = new List<CheckResult>();
        results.AddRange(await k8sChecker.CheckAsync(cancellationToken));
        results.AddRange(await httpChecker.CheckAsync(cancellationToken));
        store.UpdateResults(results);
        await historyService.RecordAsync(results, cancellationToken);
    }
}