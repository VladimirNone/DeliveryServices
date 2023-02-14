using DbManager.Data;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Neo4j.Implementations
{
    public class RepositoryFactory: IRepositoryFactory
    {
        private readonly IGraphClient dbContext;
        private readonly IServiceProvider services;
        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();

        public IGraphClient DbContext => dbContext;

        public RepositoryFactory(IGraphClient neo4jData, IServiceProvider serviceProvider)
        {
            dbContext = neo4jData;
            services = serviceProvider;
        }

        public IRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false) where TEntity : IModel
        {
            if (hasCustomRepository)
            {
                var repo = services.GetService<IRepository<TEntity>>();
                if(repo != null)
                {
                    return repo;
                }
            }

            var typeEntity = typeof(TEntity);
            if (!repositories.ContainsKey(typeEntity)) 
            { 
                var generalRepo = new GeneralRepository<TEntity>(DbContext);
                repositories.Add(typeEntity, generalRepo);
            }

            return (IRepository<TEntity>)repositories[typeEntity];
        }
    }
}
