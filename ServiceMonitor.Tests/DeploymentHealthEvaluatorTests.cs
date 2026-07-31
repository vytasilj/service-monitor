using ServiceMonitor.App.Monitoring;

namespace ServiceMonitor.Tests;

public class DeploymentHealthEvaluatorTests
{
    [Fact]
    public void Evaluate_AllReplicasReady_ReturnsOk()
    {
        var result = DeploymentHealthEvaluator.Evaluate(desiredReplicas: 3, readyReplicas: 3);
        Assert.Equal(HealthState.Ok, result);
    }

    [Fact]
    public void Evaluate_SomeReplicasReady_ReturnsWarning()
    {
        var result = DeploymentHealthEvaluator.Evaluate(desiredReplicas: 3, readyReplicas: 1);
        Assert.Equal(HealthState.Warning, result);
    }

    [Fact]
    public void Evaluate_NoReplicasReady_ReturnsError()
    {
        var result = DeploymentHealthEvaluator.Evaluate(desiredReplicas: 3, readyReplicas: 0);
        Assert.Equal(HealthState.Error, result);
    }

    [Fact]
    public void Evaluate_ScaledToZero_ReturnsOk()
    {
        var result = DeploymentHealthEvaluator.Evaluate(desiredReplicas: 0, readyReplicas: 0);
        Assert.Equal(HealthState.Ok, result);
    }

    [Fact]
    public void Evaluate_NullReadyReplicas_TreatedAsZero()
    {
        // Kubernetes reports null (not 0) for readyReplicas when a deployment has never had any pod become ready
        var result = DeploymentHealthEvaluator.Evaluate(desiredReplicas: 2, readyReplicas: null);
        Assert.Equal(HealthState.Error, result);
    }
}