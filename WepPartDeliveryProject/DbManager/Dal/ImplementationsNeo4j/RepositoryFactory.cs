using DbManager.Dal;
using DbManager.Data;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DbManager.Neo4j.Implementations
{
    public class RepositoryFactory: IRepositoryFactory
    {
        private readonly IServiceProvider _services;
        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();
        private readonly BoltGraphClientFactory _boltGraphClientFactory;

        public RepositoryFactory(BoltGraphClientFactory boltGraphClientFactory, IServiceProvider serviceProvider)
        {
            this._boltGraphClientFactory = boltGraphClientFactory;
            this._services = serviceProvider;
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
                var generalRepo = new GeneralNeo4jRepository<TEntity>(this._boltGraphClientFactory);
                repositories.Add(typeEntity, generalRepo);
            }

            return (IGeneralRepository<TEntity>)repositories[typeEntity];
        }
    }
}
