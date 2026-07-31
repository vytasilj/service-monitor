using ServiceMonitor.App.Monitoring;

namespace ServiceMonitor.Tests;

public class HistoryTransitionEvaluatorTests
{
    [Fact]
    public void ShouldRecord_NoPreviousState_ReturnsTrue()
    {
        Assert.True(HistoryTransitionEvaluator.ShouldRecord(null, HealthState.Ok));
    }

    [Fact]
    public void ShouldRecord_SameState_ReturnsFalse()
    {
        Assert.False(HistoryTransitionEvaluator.ShouldRecord(HealthState.Ok, HealthState.Ok));
    }

    [Fact]
    public void ShouldRecord_DifferentState_ReturnsTrue()
    {
        Assert.True(HistoryTransitionEvaluator.ShouldRecord(HealthState.Ok, HealthState.Error));
    }
}