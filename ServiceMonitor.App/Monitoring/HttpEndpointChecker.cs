using System.Net.Http;
using ServiceMonitor.App.Configuration;

namespace ServiceMonitor.App.Monitoring;

public class HttpEndpointChecker(MonitorConfig config, IHttpClientFactory httpClientFactory)
{
    public async Task<List<CheckResult>> CheckAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<CheckResult>();
        var client = httpClientFactory.CreateClient(nameof(HttpEndpointChecker));

        foreach (var endpoint in config.HttpEndpoints)
        {
            var checkedAt = DateTime.UtcNow;
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(endpoint.TimeoutSeconds));

                using var response = await client.GetAsync(endpoint.Url, cts.Token);
                var state = response.IsSuccessStatusCode ? HealthState.Ok : HealthState.Error;
                results.Add(new CheckResult("HTTP", endpoint.Name, state, $"HTTP {(int)response.StatusCode}", checkedAt));
            }
            catch (OperationCanceledException)
            {
                results.Add(new CheckResult("HTTP", endpoint.Name, HealthState.Error, "Timed out", checkedAt));
            }
            catch (Exception ex)
            {
                results.Add(new CheckResult("HTTP", endpoint.Name, HealthState.Error, ex.Message, checkedAt));
            }
        }

        return results;
    }
}