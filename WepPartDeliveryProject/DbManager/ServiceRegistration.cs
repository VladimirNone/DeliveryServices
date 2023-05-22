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
                        if(Convert.ToBoolean(configuration.GetSection("ApplicationSettings:GenerateData").Value) == false)
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
            services.AddTransient<IGeneralRepository<DeliveryMan>, DeliveryManRepository>();
        }

        private static void PrepareData(IGraphClient graphClient, string pathToPublicClientAppDirectory, string dirWithDishImages)
        {
            var categoryRepo = new GeneralRepository<Category>(graphClient);
            var dishRepo = new GeneralRepository<Dish>(graphClient);

            OrderState.OrderStatesFromDb = new GeneralRepository<OrderState>(graphClient).GetNodesAsync().Result;

            Category.CategoriesFromDb = categoryRepo.GetNodesAsync().Result;

            foreach (var category in Category.CategoriesFromDb)
            {
                var categoryDishes = categoryRepo.GetRelationsOfNodesAsync<ContainsDish, Dish>(category).Result.Select(h=>(Dish)h.NodeTo);

                foreach (var dish in categoryDishes)
                {
                    var pathToDishDir = PathToDirWithDish(pathToPublicClientAppDirectory, dirWithDishImages, category.LinkName, dish.Id.ToString());
                    if (Directory.Exists(pathToDishDir))
                    {
                        dish.Images = Directory
                            .GetFiles(pathToDishDir)
                            //получаемый путь
                            // /dishes/{Название категории на англ}/{Guid}/{Название файла}
                            .Select(h => ConvertFromIOPathToInternetPath_DirWithDish(pathToPublicClientAppDirectory, h))
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

        public static string PathToDirWithDish(string pathToPublicClientAppDirectory, string dirWithDishImages, string categoryLink, string dishId)
        {
            var pathToDishesDir = Path.Combine(pathToPublicClientAppDirectory, dirWithDishImages);
            var pathToCategoryDir = Path.Combine(pathToDishesDir, categoryLink);
            var pathToDishDir = Path.Combine(pathToCategoryDir, dishId);

            return pathToDishDir;
        }

        public static string ConvertFromIOPathToInternetPath_DirWithDish(string pathToPublicClientAppDirectory, string pathToImage)
        {
            pathToImage = pathToImage
                .Replace(pathToPublicClientAppDirectory, "")
                .Replace('\\', '/');

            return Path.Combine("/", pathToImage);
        }
    }
}
