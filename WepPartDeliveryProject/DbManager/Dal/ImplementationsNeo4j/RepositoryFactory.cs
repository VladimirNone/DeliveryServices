﻿using DbManager.Dal;
using DbManager.Data;
using DbManager.Neo4j.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Neo4jClient;

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

            var res = neo4jData.JsonConverters;
            var count = res.Count;
        }

        public IGeneralRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false) where TEntity : INode
        {
            if (hasCustomRepository)
            {
                var repo = services.GetService<IGeneralRepository<TEntity>>();
                if(repo != null)
                {
                    return repo;
                }
            }

            var typeEntity = typeof(TEntity);
            if (!repositories.ContainsKey(typeEntity)) 
            { 
                var generalRepo = new GeneralNeo4jRepository<TEntity>(DbContext);
                repositories.Add(typeEntity, generalRepo);
            }

            return (IGeneralRepository<TEntity>)repositories[typeEntity];
        }
    }
}