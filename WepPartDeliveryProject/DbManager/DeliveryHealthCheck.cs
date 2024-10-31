using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DbManager
{
    public class DeliveryHealthCheck : IHealthCheck
    {
        public bool StartupCompleted { get; set; }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (this.StartupCompleted)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Delivery readies."));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("Delivery isn't ready."));
        }
    }
}
