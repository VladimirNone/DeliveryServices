using DbManager.Dal;
using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Neo4j.DataGenerator;
using DbManager.Neo4j.Implementations;
using DbManager.Neo4j.Interfaces;
using DbManager.Services;
using DbManager.Neo4j.DataGenerator;
using Microsoft.Extensions.Configuration;
using DbManager.Data.Relations;
using DbManager.Data;
using DbManager.Dal;
using DbManager.AppSettings;
using DbManager.Data.Cache;

namespace DbManager
{
    public static class ServiceRegistration
    {
        public static void AddDbInfrastructure(this IServiceCollection services)
        {
            // This is to register Neo4j Client Object as a singleton
            services.AddSingleton<BoltGraphClientFactory>();

            services.AddSingleton<KafkaClientHandle>();
            services.AddSingleton<KafkaDependentProducer<string, string>>();
            services.AddSingleton<KafkaCacheEventProducer>();

            services.AddSingleton<IRepositoryFactory, RepositoryFactory>();

            services.AddSingleton<ObjectCasheKafkaChanger>();

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
