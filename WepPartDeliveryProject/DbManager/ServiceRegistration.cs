using DbManager.Neo4j.Implementations;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Neo4jClient;
using DbManager.Data.Nodes;
using DbManager.Services;
using DbManager.Neo4j.DataGenerator;
using Microsoft.Extensions.Configuration;
using DbManager.Data.Relations;
using DbManager.Data;

namespace DbManager
{
    public static class ServiceRegistration
    {
        public static void AddDbInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Fetch settings object from configuration
            var settings = new Neo4jSettings();
            configuration.GetSection("Neo4jSettings").Bind(settings);

            // This is to register Neo4j Client Object as a singleton
            services.AddSingleton<IGraphClient, BoltGraphClient>(op => {
                        var graphClient = new BoltGraphClient(settings.Neo4jConnection, settings.Neo4jUser, settings.Neo4jPassword);
                        graphClient.ConnectAsync().Wait();
                        PrepareData(graphClient, configuration.GetSection("ClientAppSettings:PathToPublicSourceDirecroty").Value, configuration.GetSection("ClientAppSettings:DirectoryWithDishImages").Value);
                        return graphClient;
                    });

            services.AddSingleton<IRepositoryFactory, RepositoryFactory>();

            services.AddTransient<IPasswordService, PasswordService>();

            services.AddTransient<DataGenerator>();
            services.AddSingleton<GeneratorService>();

            // This is the registration for custom repository class
            services.AddTransient<IGeneralRepository<Order>, OrderRepository>();
            services.AddTransient<IGeneralRepository<Dish>, DishRepository>();
            services.AddTransient<IGeneralRepository<User>, UserRepository>();
            services.AddTransient<IGeneralRepository<Client>, ClientRepository>();
        }

        private static void PrepareData(IGraphClient graphClient, string pathToPublicClientAppDirectory, string dirWithDishImages)
        {
            var categoryRepo = new GeneralRepository<Category>(graphClient);
            var dishRepo = new GeneralRepository<Dish>(graphClient);

            var pathToDishesDir = Path.Combine(pathToPublicClientAppDirectory, dirWithDishImages);

            OrderState.OrderStatesFromDb = new GeneralRepository<OrderState>(graphClient).GetNodesAsync().Result;

            Category.CategoriesFromDb = categoryRepo.GetNodesAsync().Result;

            foreach (var category in Category.CategoriesFromDb)
            {
                var categoryDishes = categoryRepo.GetRelationsOfNodesAsync<ContainsDish, Dish>(category).Result.Select(h=>(Dish)h.NodeTo);
                var pathToCategoryDir = Path.Combine(pathToDishesDir, category.LinkName);

                foreach (var dish in categoryDishes)
                {
                    var pathToDishDir = Path.Combine(pathToCategoryDir, dish.Id.ToString());
                    if (Directory.Exists(pathToDishDir))
                    {
                        dish.Images = Directory
                            .GetFiles(pathToDishDir)
                            //получаемый путь
                            // /dishes/{Название категории на англ}/{Guid}/{Название файла}
                            .Select(h => Path.Combine("\\", dirWithDishImages, category.LinkName, dish.Id.ToString(), Path.GetFileName(h)).Replace('\\','/'))
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
