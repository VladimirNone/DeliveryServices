using DbManager;
using DbManager.Data.Cache;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Interfaces;

namespace OrderWorker.BackgroundServices
{
    public class StartupBackgroundService : BackgroundService
    {
        private DeliveryHealthCheck _deliveryHealthCheck { get; set; }
        private IRepositoryFactory _repoFactory;
        private ILogger<StartupBackgroundService> _logger;
        private Instrumentation _instrumentation;

        public StartupBackgroundService(DeliveryHealthCheck deliveryHealthCheck,
            IRepositoryFactory repositoryFactory, Instrumentation instrumentation, ILogger<StartupBackgroundService> logger)
        {
            this._deliveryHealthCheck = deliveryHealthCheck;
            this._repoFactory = repositoryFactory;
            this._logger = logger;
            this._instrumentation = instrumentation;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                using var activity = this._instrumentation.ActivitySource.StartActivity($"{nameof(StartupBackgroundService)}");

                this._logger.LogInformation("Start loading containers");
                ObjectCache<Category>.Instance.AddList(await this._repoFactory.GetRepository<Category>().GetNodesAsync());
                ObjectCache<OrderState>.Instance.AddList(await this._repoFactory.GetRepository<OrderState>().GetNodesAsync());
                ObjectCache<Dish>.Instance.AddList(await this._repoFactory.GetRepository<Dish>().GetNodesAsync());
                ObjectCache<DeliveryMan>.Instance.AddList(await this._repoFactory.GetRepository<DeliveryMan>().GetNodesAsync());
                ObjectCache<Kitchen>.Instance.AddList(await this._repoFactory.GetRepository<Kitchen>().GetNodesAsync());
                ObjectCache<KitchenWorker>.Instance.AddList(await this._repoFactory.GetRepository<KitchenWorker>().GetNodesAsync());
                ObjectCache<Client>.Instance.AddList(await this._repoFactory.GetRepository<Client>().GetNodesAsync());
                ObjectCache<Admin>.Instance.AddList(await this._repoFactory.GetRepository<Admin>().GetNodesAsync());
                this._logger.LogInformation("Finish loading containers");
            });
            this._deliveryHealthCheck.StartupCompleted = true;
        }
    }
}
