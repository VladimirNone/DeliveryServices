using DbManager.Data;
using DbManager.Neo4j.Interfaces;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Neo4j.Implementations
{
    public class GeneralRepository<TEntity> : IRepository<TEntity> where TEntity : IModel
    {
        private readonly IGraphClient dbContext;

        public GeneralRepository(IGraphClient DbContext)
        {
            dbContext = DbContext;
        }

    }
}
