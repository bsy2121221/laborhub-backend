using Labor.DataAccess.IRepositories;
using Labor.DataAccess.IServices;
using Labor.Models.DTOs.Order;

namespace Labor.DataAccess.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            this._orderRepository = orderRepository;
        }
        public Task<bool> AddOrderTrackingAsync(int orderId, string status, string? description, string? location, int? createdBy)
        {
            return _orderRepository.AddOrderTrackingAsync(orderId, status, description, location, createdBy);
        }

        public Task<(int OrderId, string OrderNumber)> CreateOrderFromCartAsync(int employerId, int workAddressId, DateTime? scheduledDate)
        {
            return _orderRepository.CreateOrderFromCartAsync(employerId, workAddressId, scheduledDate);
        }

        public Task<IEnumerable<dynamic>> GetLaborOrdersAsync(int userId, string? orderStatus = null, int pageNumber = 1, int pageSize = 20)
        {
           return _orderRepository.GetLaborOrdersAsync(userId, orderStatus, pageNumber, pageSize);
        }

        public Task<OrderResponseDto?> GetOrderDetailsAsync(int orderId, int? userId = null)
        {
           return _orderRepository.GetOrderDetailsAsync(orderId, userId);
        }

        public Task<IEnumerable<dynamic>> GetUserOrdersAsync(int userId, string? orderStatus = null, int pageNumber = 1, int pageSize = 20)
        {
            return _orderRepository.GetUserOrdersAsync(userId, orderStatus, pageNumber, pageSize);
        }

        public Task<bool> UpdateOrderItemStatusAsync(int orderItemId, string itemStatus, int? actualHours = null, DateTime? startTime = null, DateTime? endTime = null, int? updatedBy = null)
        {
            return _orderRepository.UpdateOrderItemStatusAsync(orderItemId, itemStatus, actualHours, startTime, endTime, updatedBy);
        }

        public Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus, string? description, int updatedBy)
        {
            return _orderRepository.UpdateOrderStatusAsync(orderId, newStatus, description, updatedBy);
        }
    }
}
