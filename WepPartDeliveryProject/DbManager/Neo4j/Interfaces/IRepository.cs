using DbManager.Data;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Neo4j.Interfaces
{
    public interface IRepository<TEntity> where TEntity : Model
    {
        Task AddNodeAsync(TEntity entity);
        Task UpdateNodeAsync(TEntity entity);
        Task<TEntity> GetNodeAsync(int id);
        Task<List<TEntity>> GetNodesAsync();
        Task DeleteNodeWithRelations(TEntity entity);
    }
}
