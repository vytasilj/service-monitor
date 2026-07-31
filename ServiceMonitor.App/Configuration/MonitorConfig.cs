namespace ServiceMonitor.App.Configuration;

public class MonitorConfig
{
    public string KubeConfigPath { get; set; } = "";
    public int PollIntervalSeconds { get; set; } = 30;
    public List<NamespaceWatchConfig> Namespaces { get; set; } = [];
    public List<HttpEndpointConfig> HttpEndpoints { get; set; } = [];
}
