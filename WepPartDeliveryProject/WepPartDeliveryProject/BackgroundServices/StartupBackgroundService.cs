
using DbManager;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Helpers;
using DbManager.Neo4j.DataGenerator;
using DbManager.Neo4j.Implementations;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.Options;
using Neo4jClient;

namespace WepPartDeliveryProject.BackgroundServices
{
    public class StartupBackgroundService : BackgroundService
    {
        private DeliveryHealthCheck _deliveryHealthCheck { get; set; }
        private BoltGraphClientFactory _boltGraphClientFactory { get; set; }
        private IConfiguration _configuration { get; set; }
        private ApplicationSettings _appSettings { get; set; }
        private GeneratorService _generatorService { get; set; }
        private IRepositoryFactory _repoFactory;
        private Instrumentation _instrumentation;

        public StartupBackgroundService(DeliveryHealthCheck deliveryHealthCheck, BoltGraphClientFactory boltGraphClientFactory, IConfiguration configuration, 
            IOptions<ApplicationSettings> appSettingsOptions, GeneratorService generatorService, IRepositoryFactory repositoryFactory, Instrumentation instrumentation)
        {
            this._deliveryHealthCheck = deliveryHealthCheck;
            this._boltGraphClientFactory = boltGraphClientFactory;
            this._configuration = configuration;
            this._appSettings = appSettingsOptions.Value;
            this._generatorService = generatorService;
            this._repoFactory = repositoryFactory;
            this._instrumentation = instrumentation;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                using var activity = this._instrumentation.ActivitySource.StartActivity($"{nameof(StartupBackgroundService)}");
                var graphClient = await _boltGraphClientFactory.GetGraphClientAsync();
                if (this._appSettings.GenerateData)
                {
                    await this._generatorService.GenerateAll();
                }
                await this.PrepareData( this._configuration.GetSection("ClientAppSettings:PathToPublicSourceDirecroty")?.Value, 
                                        this._configuration.GetSection("ClientAppSettings:DirectoryWithDishImages")?.Value);
                
            }, stoppingToken);
            this._deliveryHealthCheck.StartupCompleted = true;
        }

        private async Task PrepareData(string pathToPublicClientAppDirectory, string dirWithDishImages)
        {
            if (pathToPublicClientAppDirectory == null || dirWithDishImages == null)
                throw new ArgumentException("PathToPublicSourceDirecroty or DirectoryWithDishImages in appsettings.json (or envs) is null. We can't generate data.");

            var categoryRepo = this._repoFactory.GetRepository<Category>();
            var dishRepo = this._repoFactory.GetRepository<Dish>();

            OrderState.OrderStatesFromDb = await this._repoFactory.GetRepository<OrderState>().GetNodesAsync();

            Category.CategoriesFromDb = await categoryRepo.GetNodesAsync();

            foreach (var category in Category.CategoriesFromDb)
            {
                var categoryDishes = (await categoryRepo.GetRelationsOfNodesAsync<ContainsDish, Dish>(category)).Select(h => (Dish)h.NodeTo);

                foreach (var dish in categoryDishes)
                {
                    var pathToDishDir = FilePathHelper.PathToDirWithDish(pathToPublicClientAppDirectory, dirWithDishImages, category.LinkName, dish.Id.ToString());
                    if (Directory.Exists(pathToDishDir))
                    {
                        dish.Images = Directory
                            .GetFiles(pathToDishDir)
                            //получаемый путь
                            // /dishes/{Название категории на англ}/{Guid}/{Название файла}
                            .Select(h => FilePathHelper.ConvertFromIOPathToInternetPath_DirWithDish(pathToPublicClientAppDirectory, h))
                            .ToList();

                        dishRepo.UpdateNodeAsync(dish).Wait();
                    }
                    else
                    {
                        Directory.CreateDirectory(pathToDishDir);
                    }
                }
            }

        }
    }
}
