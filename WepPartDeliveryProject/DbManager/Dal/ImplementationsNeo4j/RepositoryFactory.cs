using DbManager.Dal;
using DbManager.Dal.ImplementationsKafka;
using DbManager.Data;
using DbManager.Neo4j.Interfaces;
using DbManager.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DbManager.Neo4j.Implementations
{
    public class RepositoryFactory: IRepositoryFactory
    {
        private readonly IServiceProvider _services;
        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();
        private readonly KafkaDependentProducer<string, string> _kafkaDependentProducer;
        private readonly IConfiguration _configuration;
        private readonly BoltGraphClientFactory _boltGraphClientFactory;

        public RepositoryFactory(BoltGraphClientFactory boltGraphClientFactory, KafkaDependentProducer<string, string> kafkaDependentProducer, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            this._boltGraphClientFactory = boltGraphClientFactory;
            this._services = serviceProvider;
            this._kafkaDependentProducer = kafkaDependentProducer;
            this._configuration = configuration;
        }

        public IGeneralRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false) where TEntity : INode
        {
            if (hasCustomRepository)
            {
                var repo = _services.GetService<IGeneralRepository<TEntity>>();
                if(repo != null)
                {
                    return repo;
                }
            }

            var typeEntity = typeof(TEntity);
            if (!repositories.ContainsKey(typeEntity)) 
            { 
                var generalRepo = new GeneralKafkaRepository<TEntity>(this._boltGraphClientFactory, this._kafkaDependentProducer, this._configuration);
                repositories.Add(typeEntity, generalRepo);
            }

            return (IGeneralRepository<TEntity>)repositories[typeEntity];
        }
    }
}
