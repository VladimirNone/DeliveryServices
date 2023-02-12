using DbManager.Neo4j.Implementations;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;
namespace DbManager
{
    public static class ServiceRegistration
    {
        public static void AddDbInfrastructure(this IServiceCollection services, ApplicationSettings settings)
        {
            // This is to register Neo4j Driver Object as a singleton
            services.AddSingleton(GraphDatabase.Driver(settings.Neo4jConnection, AuthTokens.Basic(settings.Neo4jUser, settings.Neo4jPassword)));

            // This is Data Access Wrapper over Neo4j session, that is a helper class for executing parameterized Neo4j Cypher queries in Transactions
            services.AddScoped<INeo4jDataAccess, Neo4jDataAccess>();

            // This is the registration for domain repository class
            //services.AddTransient<IPersonRepository, PersonRepository>();
        }
    }
}
