using DbManager.AppSettings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neo4jClient;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;

namespace DbManager
{
    public class BoltGraphClientFactory
    {
        private readonly Neo4jSettings _neo4jSettings;
        private readonly ILogger<BoltGraphClientFactory> _logger;
        private readonly Instrumentation _instrumentation;
        private readonly Counter<long> _databaseOperationCounter;

        public BoltGraphClientFactory(IOptions<Neo4jSettings> neo4JSettingsOptions, Instrumentation instrumentation, ILogger<BoltGraphClientFactory> logger)
        {
            this._neo4jSettings = neo4JSettingsOptions.Value;
            this._logger = logger;
            this._instrumentation = instrumentation;
            this._databaseOperationCounter = this._instrumentation.Meter.CreateCounter<long>("database.operations", description: "The number of operations completed on database.");
        }

        public IGraphClient GetGraphClient()
        {
            var graphClient = new BoltGraphClient(this._neo4jSettings.Neo4jConnection, this._neo4jSettings.Neo4jUser, this._neo4jSettings.Neo4jPassword);
            graphClient.ConnectAsync().Wait();
            graphClient.OperationCompleted += GraphClient_OperationCompleted;
            return graphClient;
        }

        public async Task<IGraphClient> GetGraphClientAsync()
        {
            var graphClient = new BoltGraphClient(this._neo4jSettings.Neo4jConnection, this._neo4jSettings.Neo4jUser, this._neo4jSettings.Neo4jPassword);
            await graphClient.ConnectAsync();
            graphClient.OperationCompleted += GraphClient_OperationCompleted;
            return graphClient;
        }

        private void GraphClient_OperationCompleted(object sender, OperationCompletedEventArgs e)
        {
            this._databaseOperationCounter.Add(1);
            this._logger.LogTrace(Regex.Replace(e.QueryText.Replace("\\r\\n", " "), @"PasswordHash:\s*\[.*?\]", "PasswordHash: [*]", RegexOptions.Singleline));
        }
    }
}
