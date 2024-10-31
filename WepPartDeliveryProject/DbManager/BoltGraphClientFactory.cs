using DbManager.AppSettings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neo4jClient;

namespace DbManager
{
    public class BoltGraphClientFactory
    {
        private Neo4jSettings _neo4jSettings;
        private ILogger<BoltGraphClientFactory> _logger;

        public BoltGraphClientFactory(IOptions<Neo4jSettings> neo4JSettingsOptions, ILogger<BoltGraphClientFactory> logger)
        {
            this._neo4jSettings = neo4JSettingsOptions.Value;
            this._logger = logger;
        }

        public IGraphClient GetGraphClient()
        {
            var graphClient = new BoltGraphClient(this._neo4jSettings.Neo4jConnection, this._neo4jSettings.Neo4jUser, this._neo4jSettings.Neo4jPassword);
            graphClient.ConnectAsync().Wait();
            graphClient.OperationCompleted += (sender, e) => this._logger.LogTrace(e.QueryText.Replace("\r\n", " "));
            return graphClient;
        }

        public async Task<IGraphClient> GetGraphClientAsync()
        {
            var graphClient = new BoltGraphClient(this._neo4jSettings.Neo4jConnection, this._neo4jSettings.Neo4jUser, this._neo4jSettings.Neo4jPassword);
            await graphClient.ConnectAsync();
            graphClient.OperationCompleted += (sender, e) => this._logger.LogTrace(e.QueryText.Replace("\r\n", " "));
            return graphClient;
        }
    }
}
