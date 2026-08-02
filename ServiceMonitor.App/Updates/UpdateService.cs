using Velopack;
using Velopack.Sources;

namespace ServiceMonitor.App.Updates;

public class UpdateService
{
    private readonly UpdateManager _manager;

    public UpdateService(string githubRepoUrl)
    {
        // accessToken: null is fine for a public repository.
        var source = new GithubSource(githubRepoUrl, null, prerelease: false);
        _manager = new UpdateManager(source);
    }

    public async Task CheckDownloadAndApplyAsync(CancellationToken cancellationToken = default)
    {
        if (!_manager.IsInstalled)
        {
            // Running via `dotnet run`, not through the installed/packaged app — nothing to update.
            return;
        }

        var updateInfo = await _manager.CheckForUpdatesAsync();
        if (updateInfo is null)
        {
            return;
        }

        await _manager.DownloadUpdatesAsync(updateInfo, cancelToken: cancellationToken);

        // This exits the process immediately, applies the update, and relaunches the app.
        _manager.ApplyUpdatesAndRestart(updateInfo);
    }
}