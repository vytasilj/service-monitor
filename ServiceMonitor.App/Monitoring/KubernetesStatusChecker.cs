using k8s;
using ServiceMonitor.App.Configuration;

namespace ServiceMonitor.App.Monitoring;

public class KubernetesStatusChecker(MonitorConfig config)
{
    public async Task<List<CheckResult>> CheckAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<CheckResult>();

        if (config.Namespaces.Count == 0)
        {
            return results;
        }

        IKubernetes client;
        try
        {
            var kubeConfig = KubernetesClientConfiguration.BuildConfigFromConfigFile(config.KubeConfigPath);
            client = new Kubernetes(kubeConfig);
        }
        catch (Exception ex)
        {
            // Can't even build a client (e.g. bad kubeconfig path) — report it as a single error, not a crash.
            results.Add(new CheckResult("Kubernetes", "cluster connection", HealthState.Error, ex.Message, DateTime.UtcNow));
            return results;
        }

        foreach (var ns in config.Namespaces)
        {
            try
            {
                var deployments = await client.AppsV1.ListNamespacedDeploymentAsync(ns.Namespace, cancellationToken: cancellationToken);

                var relevant = ns.WatchAllDeployments
                    ? deployments.Items
                    : deployments.Items.Where(d => ns.SpecificDeployments.Contains(d.Metadata.Name));

                foreach (var deployment in relevant)
                {
                    var state = DeploymentHealthEvaluator.Evaluate(deployment.Spec.Replicas, deployment.Status.ReadyReplicas);
                    var detail = $"{deployment.Status.ReadyReplicas ?? 0}/{deployment.Spec.Replicas ?? 0} replicas ready";

                    results.Add(new CheckResult(
                        "Kubernetes",
                        $"{ns.Namespace}/{deployment.Metadata.Name}",
                        state,
                        detail,
                        DateTime.UtcNow));
                }
            }
            catch (Exception ex)
            {
                results.Add(new CheckResult("Kubernetes", $"namespace/{ns.Namespace}", HealthState.Error, ex.Message, DateTime.UtcNow));
            }
        }

        return results;
    }
}