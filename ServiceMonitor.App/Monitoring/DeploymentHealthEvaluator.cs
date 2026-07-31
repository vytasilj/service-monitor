namespace ServiceMonitor.App.Monitoring;

public static class DeploymentHealthEvaluator
{
    public static HealthState Evaluate(int? desiredReplicas, int? readyReplicas)
    {
        var desired = desiredReplicas ?? 0;
        var ready = readyReplicas ?? 0;

        // Intentionally scaled to zero (e.g. a paused staging deployment) is not an error.
        if (desired == 0) return HealthState.Ok;

        if (ready == 0) return HealthState.Error;
        if (ready < desired) return HealthState.Warning;

        return HealthState.Ok;
    }
}