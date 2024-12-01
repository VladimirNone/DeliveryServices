using DbManager.AppSettings;
using DbManager.Dal;
using DbManager.Dal.ImplementationsKafka;
using DbManager.Data;
using DbManager.Neo4j.Interfaces;
using DbManager.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace DbManager.Neo4j.Implementations
{
    public class RepositoryFactory: IRepositoryFactory
    {
        private readonly IServiceProvider _services;
        private readonly ConcurrentDictionary<Type, object> repositories = new ConcurrentDictionary<Type, object>();
        private readonly KafkaCacheEventProducer _kafkaProducer;
        private readonly BoltGraphClientFactory _boltGraphClientFactory;
        private Instrumentation _instrumentation;
        private object sync = new object();

        public RepositoryFactory(BoltGraphClientFactory boltGraphClientFactory, KafkaCacheEventProducer kafkaProducer, IServiceProvider serviceProvider, Instrumentation instrumentation)
        {
            this._boltGraphClientFactory = boltGraphClientFactory;
            this._services = serviceProvider;
            this._kafkaProducer = kafkaProducer;
            this._instrumentation = instrumentation;
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
                if (repositories.TryAdd(typeEntity, repo))
                    return repo;
                else
                    return (IGeneralRepository<TEntity>)repositories[typeEntity];
            }

            repo = new GeneralKafkaRepository<TEntity>(this._boltGraphClientFactory, this._kafkaProducer, this._instrumentation);
            if (repositories.TryAdd(typeEntity, repo))
                return repo;
            else
                return (IGeneralRepository<TEntity>)repositories[typeEntity];
        }

        public IGeneralRepository GetRepository(Type typeOfNode) 
        {
            if (repositories.TryGetValue(typeOfNode, out var resRepo))
            {
                return (IGeneralRepository)resRepo;
            }

            throw new InvalidOperationException($"Repository with type {typeOfNode} don't exist or cached");
        }
    }
}
