using System.IO;
using Microsoft.Extensions.Hosting;

namespace ServiceMonitor.App.Updates;

public class UpdateCheckBackgroundService(UpdateService updateService) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(6);
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ServiceMonitor", "update-log.txt");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CheckInterval);

        do
        {
            try
            {
                Log("Checking for updates...");
                await updateService.CheckDownloadAndApplyAsync(stoppingToken);
                Log("Check finished (no update applied, or app is about to restart).");
            }
            catch (Exception ex)
            {
                Log($"Update check failed: {ex}");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private static void Log(string message)
    {
        try
        {
            File.AppendAllText(LogPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
        }
        catch
        {
            // Logging itself must never crash the app.
        }
    }
}