using DbManager.AppSettings;
using DbManager.Dal;
using DbManager.Dal.ImplementationsKafka;
using DbManager.Data;
using DbManager.Neo4j.Interfaces;
using DbManager.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DbManager.Neo4j.Implementations
{
    public class RepositoryFactory: IRepositoryFactory
    {
        private readonly IServiceProvider _services;
        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();
        private readonly KafkaDependentProducer<string, string> _kafkaDependentProducer;
        private readonly IOptions<KafkaSettings> _kafkaOptions;
        private readonly BoltGraphClientFactory _boltGraphClientFactory;

        public RepositoryFactory(BoltGraphClientFactory boltGraphClientFactory, KafkaDependentProducer<string, string> kafkaDependentProducer, IOptions<KafkaSettings> kafkaSettings, IServiceProvider serviceProvider)
        {
            this._boltGraphClientFactory = boltGraphClientFactory;
            this._services = serviceProvider;
            this._kafkaDependentProducer = kafkaDependentProducer;
            this._kafkaOptions = kafkaSettings;
        }

        public IGeneralRepository<TEntity> GetRepository<TEntity>() where TEntity : INode
        {
            var typeEntity = typeof(TEntity);

            if(repositories.TryGetValue(typeEntity, out var resRepo))
            {
                return (IGeneralRepository<TEntity>)resRepo;
            }

            var repo = _services.GetService<IGeneralRepository<TEntity>>();
            if (repo != null)
            {
                repositories.Add(typeEntity, repo);
                return repo;
            }

            repo = new GeneralKafkaRepository<TEntity>(this._boltGraphClientFactory, this._kafkaDependentProducer, this._kafkaOptions);
            repositories.Add(typeEntity, repo);

            return repo;
        }
    }
}
