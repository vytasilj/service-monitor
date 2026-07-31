namespace ServiceMonitor.App.Configuration;

public class HttpEndpointConfig
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 5;
}