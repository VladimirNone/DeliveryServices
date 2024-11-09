using DbManager.Dal;
using DbManager.Data;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DbManager.Neo4j.Implementations
{
    public class RepositoryFactory: IRepositoryFactory
    {
        private readonly IServiceProvider _services;
        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();
        private readonly BoltGraphClientFactory _boltGraphClientFactory;
        private Instrumentation _instrumentation;

        public RepositoryFactory(BoltGraphClientFactory boltGraphClientFactory, IServiceProvider serviceProvider, Instrumentation instrumentation)
        {
            this._boltGraphClientFactory = boltGraphClientFactory;
            this._services = serviceProvider;
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
                repositories.Add(typeEntity, repo);
                return repo;
            }

            repo = new GeneralNeo4jRepository<TEntity>(this._boltGraphClientFactory, this._instrumentation);
            repositories.Add(typeEntity, repo);

            return repo;
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
