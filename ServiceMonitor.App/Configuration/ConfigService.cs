using System.IO;
using System.Text.Json;

namespace ServiceMonitor.App.Configuration;

public class ConfigService
{
    private static readonly string ConfigDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ServiceMonitor");

    private static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public MonitorConfig Load()
    {
        if (!File.Exists(ConfigFilePath))
        {
            var defaultConfig = CreateDefaultConfig();
            Save(defaultConfig);
            return defaultConfig;
        }

        var json = File.ReadAllText(ConfigFilePath);
        return JsonSerializer.Deserialize<MonitorConfig>(json, JsonOptions)
               ?? CreateDefaultConfig();
    }

    public void Save(MonitorConfig config)
    {
        Directory.CreateDirectory(ConfigDirectory);
        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(ConfigFilePath, json);
    }

    public string GetConfigFilePath() => ConfigFilePath;

    private static MonitorConfig CreateDefaultConfig()
    {
        var defaultKubeConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".kube", "config");

        return new MonitorConfig
        {
            KubeConfigPath = defaultKubeConfigPath,
            PollIntervalSeconds = 30,
            Namespaces =
            [
                new NamespaceWatchConfig { Namespace = "production", WatchAllDeployments = true },
                new NamespaceWatchConfig
                {
                    Namespace = "staging",
                    WatchAllDeployments = false,
                    SpecificDeployments = ["api"]
                }
            ],
            HttpEndpoints =
            [
                new HttpEndpointConfig { Name = "Example API", Url = "https://example.com/health", TimeoutSeconds = 5 }
            ]
        };
    }
}