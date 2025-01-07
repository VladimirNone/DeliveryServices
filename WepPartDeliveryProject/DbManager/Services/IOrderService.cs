using DbManager.Data.Relations;

namespace DbManager.Services
{
    public interface IOrderService
    {
        Task<bool> MoveOrderToNextStage(string orderId, string comment);
        Task<bool> MoveOrderToPreviousStage(string orderId);
        Task CancelOrderedDish(string orderId, string dishId);
        Task CancelOrder(string orderId, string reasonOfCancel);
        Task ChangeCountOrderedDish(string orderId, string dishId, int count);
        Task PlaceAnOrder(string userId, Dictionary<string, int> dishesCounts, string comment, string phoneNumber, string deliveryAddress);
    }
}