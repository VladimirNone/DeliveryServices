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

namespace DbManager
{
    public static class ServiceRegistration
    {
        public static void AddDbInfrastructure(this IServiceCollection services, ApplicationSettings settings)
        {
            // This is to register Neo4j Client Object as a singleton
            services.AddSingleton<IGraphClient, BoltGraphClient>(op => {
                    var graphClient = new BoltGraphClient(settings.Neo4jConnection, settings.Neo4jUser, settings.Neo4jPassword);
                    graphClient.ConnectAsync().Wait();
                    graphClient.OperationCompleted += GraphClient_OperationLogger;
                    LoadStandartData(graphClient);
                    return graphClient;
                });

            services.AddSingleton<IRepositoryFactory, RepositoryFactory>();

            services.AddTransient<IPasswordService, PasswordService>();

            services.AddTransient<DataGenerator>();
            services.AddSingleton<GeneratorService>();

            // This is the registration for domain repository class
            //services.AddTransient<IPersonRepository, PersonRepository>();

        }

        private static void GraphClient_OperationLogger(object sender, OperationCompletedEventArgs e)
        {
            
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
                .ToDictionary(h => h.NameOfState);
        }
    }
}
