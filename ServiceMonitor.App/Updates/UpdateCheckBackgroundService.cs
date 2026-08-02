using Microsoft.Extensions.Hosting;

namespace ServiceMonitor.App.Updates;

public class UpdateCheckBackgroundService(UpdateService updateService) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CheckInterval);

        do
        {
            try
            {
                await updateService.CheckDownloadAndApplyAsync(stoppingToken);
            }
            catch
            {
                // An update check failing (e.g. no internet) should never crash the app —
                // it'll simply try again at the next interval.
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}