using ServiceMonitor.App.Monitoring;

namespace ServiceMonitor.Tests;

public class ResultFilterTests
{
    [Fact]
    public void Matches_NoFilters_ReturnsTrue()
    {
        Assert.True(ResultFilter.Matches("api", HealthState.Ok, null, null));
    }

    [Fact]
    public void Matches_StateFilterMismatch_ReturnsFalse()
    {
        Assert.False(ResultFilter.Matches("api", HealthState.Ok, null, HealthState.Error));
    }

    [Fact]
    public void Matches_NameFilter_IsCaseInsensitiveSubstring()
    {
        Assert.True(ResultFilter.Matches("Production/API", HealthState.Ok, "api", null));
    }

    [Fact]
    public void Matches_NameFilter_NoMatch_ReturnsFalse()
    {
        Assert.False(ResultFilter.Matches("worker", HealthState.Ok, "api", null));
    }
}