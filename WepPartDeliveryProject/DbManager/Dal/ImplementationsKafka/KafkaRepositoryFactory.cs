using DbManager.AppSettings;
using DbManager.Dal;
using DbManager.Data;
using DbManager.Neo4j.Interfaces;
using DbManager.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace DbManager.Dal.ImplementationsKafka
{
    public class KafkaRepositoryFactory : IRepositoryFactory
    {
        private readonly IServiceProvider _services;
        private readonly ConcurrentDictionary<Type, object> repositories = new ConcurrentDictionary<Type, object>();
        private readonly KafkaEventProducer _kafkaProducer;
        private readonly BoltGraphClientFactory _boltGraphClientFactory;
        private Instrumentation _instrumentation;

        public KafkaRepositoryFactory(BoltGraphClientFactory boltGraphClientFactory, KafkaEventProducer kafkaProducer, IServiceProvider serviceProvider, Instrumentation instrumentation)
        {
            _boltGraphClientFactory = boltGraphClientFactory;
            _services = serviceProvider;
            _kafkaProducer = kafkaProducer;
            _instrumentation = instrumentation;
        }

        public IGeneralRepository<TEntity> GetRepository<TEntity>() where TEntity : INode
        {
            var typeEntity = typeof(TEntity);

            if (repositories.TryGetValue(typeEntity, out var resRepo))
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

            repo = new GeneralKafkaRepository<TEntity>(_boltGraphClientFactory, _kafkaProducer, _instrumentation);
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
