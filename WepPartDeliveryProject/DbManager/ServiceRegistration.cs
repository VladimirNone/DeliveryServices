using DbManager.Neo4j.Implementations;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;
using Neo4jClient;

namespace DbManager
{
    public static class ServiceRegistration
    {
        public static void AddDbInfrastructure(this IServiceCollection services, ApplicationSettings settings)
        {
            // This is to register Neo4j Client Object as a singleton
            services.AddSingleton<IGraphClient, BoltGraphClient>(op => new BoltGraphClient(settings.Neo4jConnection, settings.Neo4jUser, settings.Neo4jPassword));

            // This is the registration for domain repository class
            //services.AddTransient<IPersonRepository, PersonRepository>();
        }
    }
}
