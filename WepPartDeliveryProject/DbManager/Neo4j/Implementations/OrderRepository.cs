using DbManager.Data;
using DbManager.Data.Nodes;
using DbManager.Data.Relations;
using DbManager.Neo4j.Interfaces;
using Neo4jClient;
using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Neo4j.Implementations
{
    public class OrderRepository : GeneralRepository<Order>, IOrderRepository
    {
        public OrderRepository(IGraphClient DbContext) : base(DbContext)
        {
        }

        public async Task<List<Order>> GetOrdersByState(string kitchenId, string orderStateId)
        {
            return await GetOrdersByState(Guid.Parse(kitchenId), Guid.Parse(orderStateId));
        }

        public async Task<List<Order>> GetOrdersByState(Guid kitchenId, Guid orderStateId)
        {
            var directionInOrder = GetDirection(typeof(CookedBy).Name.ToUpper(), false);
            var directionInOrderState = GetDirection(typeof(HasOrderState).Name.ToUpper(), true);

            var res = await dbContext.Cypher
                .Match(
                    $"(kitchen:{typeof(Kitchen).Name} {{Id: $kitchenId}})" +
                    $"{directionInOrder}" +
                    $"(orders:{typeof(Order).Name})" +
                    $"{directionInOrderState}" +
                    $"(orderState:{typeof(OrderState)} {{Id: $orderStateId}})")
                .WithParams(new
                    {
                        kitchenId,
                        orderStateId,
                })
                .Return(orders => orders.CollectAs<Order>())
                .ResultsAsync;

            return res.Single().ToList();
        }

        public async Task MoveOrderToNextStage(Order order)
        {
            throw new NotImplementedException();
        }
    }
}
