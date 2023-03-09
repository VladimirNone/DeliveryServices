using DbManager.Neo4j.Implementations;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Neo4jClient;
using DbManager.Data.Nodes;
using System.Collections;
using DbManager.Services;
using DbManager.Neo4j.DataGenerator;
using DbManager.Neo4j;
using Neo4j.Driver;

namespace DbManager
{
    public static class ServiceRegistration
    {
        public static void AddDbInfrastructure(this IServiceCollection services, Neo4jSettings settings)
        {
            // This is to register Neo4j Client Object as a singleton
            services.AddSingleton<IGraphClient, BoltGraphClient>(op => {
                    var graphClient = new BoltGraphClient(settings.Neo4jConnection, settings.Neo4jUser, settings.Neo4jPassword);
                    graphClient.ConnectAsync().Wait();
                    LoadStandartData(graphClient);
                    return graphClient;
                    });

            services.AddSingleton<IRepositoryFactory, RepositoryFactory>();

            services.AddTransient<IPasswordService, PasswordService>();

            services.AddTransient<DataGenerator>();
            services.AddSingleton<GeneratorService>();

            // This is the registration for custom repository class
            services.AddTransient<IGeneralRepository<Order>, OrderRepository>();
        }

        private static void LoadStandartData(IGraphClient graphClient)
        {
            OrderState.OrderStatesFromDb =
                graphClient.Cypher
                .Match($"(orderStates:{typeof(OrderState).Name})")
                .Return(orderStates => orderStates.CollectAs<OrderState>())
                .ResultsAsync
                .Result
                .Single()
                .ToList();
        }
    }
}
