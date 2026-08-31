namespace ServiceMonitor.App.Services;

public interface IStartupService
{
    bool IsRunAtStartupEnabled();
    void SetRunAtStartup(bool enable);
}
