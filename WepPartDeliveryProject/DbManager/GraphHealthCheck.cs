using Microsoft.Extensions.Diagnostics.HealthChecks;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager
{
    public class GraphHealthCheck : IHealthCheck
    {

        private readonly IGraphClient _client;
        public GraphHealthCheck(IGraphClient client)
        {
            _client = client;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {

            var healthCheckResultHealthy = await CheckNeo4jGraphConnectionAsync();


            if (healthCheckResultHealthy)
            {
                return HealthCheckResult.Healthy("neo4j graph db health check success");
            }

            return HealthCheckResult.Unhealthy("neo4j graph db health check success"); ;
        }

        private async Task<bool> CheckNeo4jGraphConnectionAsync()
        {
            try
            {
                await _client.ConnectAsync();
            }

            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
