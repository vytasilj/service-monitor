namespace ServiceMonitor.App.Configuration;

public class NamespaceWatchConfig
{
    public string Namespace { get; set; } = "";
    public bool WatchAllDeployments { get; set; } = true;

    // Only used when WatchAllDeployments is false — an explicit allowlist of
    // deployment names within this namespace (e.g. staging: watch only "api", not everything).
    public List<string> SpecificDeployments { get; set; } = [];
}
