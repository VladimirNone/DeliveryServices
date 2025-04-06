using DbManager.Data;

using DbManager.Data.Kafka;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Interfaces;

namespace DbManager.Services.Kafka
{
    public class OrderKafkaService : IOrderService
    {
        private readonly KafkaEventProducer _kafkaProducer;
        private readonly IRepositoryFactory _repositoryFactory;

        public OrderKafkaService(IRepositoryFactory repositoryFactory, KafkaEventProducer kafkaProducer)
        {
            this._repositoryFactory = repositoryFactory;
            this._kafkaProducer = kafkaProducer;
        }

        public async Task CancelOrder(string orderId, string reasonOfCancel)
        {
            await this._kafkaProducer.ProduceOrderAsync(new KafkaChangeOrderEvent() { Order = new Order() { Id = Guid.Parse(orderId) }, MethodName = KafkaChangeOrderEvent.CancelOrderMethodName, TupleMethodParams = (orderId, reasonOfCancel) });
        }

        public async Task CancelOrderedDish(string orderId, string dishId)
        {
            await this._kafkaProducer.ProduceOrderAsync(new KafkaChangeOrderEvent() { Order = new Order() { Id = Guid.Parse(orderId) }, MethodName = KafkaChangeOrderEvent.CancelOrderedDishMethodName, TupleMethodParams = (orderId, dishId) });
        }

        public async Task ChangeCountOrderedDish(string orderId, string dishId, int count)
        {
            await this._kafkaProducer.ProduceOrderAsync(new KafkaChangeOrderEvent() { Order = new Order() { Id = Guid.Parse(orderId) }, MethodName = KafkaChangeOrderEvent.ChangeCountOrderedDishMethodName, TupleMethodParams = (orderId, dishId, count) });
        }

        public async Task<bool> MoveOrderToNextStage(string orderId, string comment)
        {
            var orderRepo = this._repositoryFactory.GetRepository<Order>();

            var order = await orderRepo.GetNodeAsync(Guid.Parse(orderId));
            var orderHasState = order.Story.Last();
            var orderState = OrderState.OrderStatesFromDb.Single(h => h.Id == orderHasState.NodeToId);

            //Если заказ был отменен или завершен, то ничего не произойдет
            if ((OrderStateEnum)orderState.NumberOfStage == OrderStateEnum.Cancelled || (OrderStateEnum)orderState.NumberOfStage == OrderStateEnum.Finished)
                return false;

            await this._kafkaProducer.ProduceOrderAsync(new KafkaChangeOrderEvent() { Order = new Order() { Id = Guid.Parse(orderId) }, MethodName = KafkaChangeOrderEvent.MoveOrderToNextStageMethodName, TupleMethodParams = (orderId, comment) });
            return true;
        }

        public async Task<bool> MoveOrderToPreviousStage(string orderId)
        {
            var orderRepo = this._repositoryFactory.GetRepository<Order>();

            var order = await orderRepo.GetNodeAsync(Guid.Parse(orderId));
            var orderHasState = order.Story.Last();
            var orderState = OrderState.OrderStatesFromDb.Single(h => h.Id == orderHasState.NodeToId);
            //Если заказ только попал в очередь
            if ((OrderStateEnum)orderState.NumberOfStage == OrderStateEnum.InQueue)
                return false;

            await this._kafkaProducer.ProduceOrderAsync(new KafkaChangeOrderEvent() { Order = new Order() { Id = Guid.Parse(orderId) }, MethodName = KafkaChangeOrderEvent.MoveOrderToPreviousStageMethodName, TupleMethodParams = orderId });
            return true;
        }

        public async Task PlaceAnOrder(string orderId, string userId, Dictionary<string, int> dishesCounts, string comment, string phoneNumber, string deliveryAddress)
        {
            await this._kafkaProducer.ProduceOrderAsync(new KafkaChangeOrderEvent() { Order = new Order() { Id = Guid.Parse(orderId) }, MethodName = KafkaChangeOrderEvent.PlaceAnOrderMethodName, TupleMethodParams = (orderId, userId, dishesCounts, comment, phoneNumber, deliveryAddress) });
        }
    }
}
