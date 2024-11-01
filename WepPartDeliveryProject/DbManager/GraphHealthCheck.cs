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

        private readonly BoltGraphClientFactory _boltGraphClientFactory;
        public GraphHealthCheck(BoltGraphClientFactory boltGraphClientFactory)
        {
            this._boltGraphClientFactory = boltGraphClientFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var healthCheckResultHealthy = await CheckNeo4jGraphConnectionAsync(await this._boltGraphClientFactory.GetGraphClientAsync());

            if (healthCheckResultHealthy)
            {
                return HealthCheckResult.Healthy("neo4j graph db health check success");
            }

            return HealthCheckResult.Unhealthy("neo4j graph db health check unsuccess"); ;
        }

        private async Task<bool> CheckNeo4jGraphConnectionAsync(IGraphClient client)
        {
            try
            {
                await client.ConnectAsync();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                client.Dispose();
            }

            return true;
        }
    }
}
