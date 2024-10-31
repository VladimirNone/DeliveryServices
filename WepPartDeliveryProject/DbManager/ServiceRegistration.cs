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
using DbManager.Dal;

namespace DbManager
{
    public static class ServiceRegistration
    {
        public static void AddDbInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // This is to register Neo4j Client Object as a singleton
            services.AddSingleton<BoltGraphClientFactory>();

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
    }
}
