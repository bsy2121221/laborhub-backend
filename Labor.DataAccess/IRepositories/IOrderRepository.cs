using Labor.Models.DTOs.Order;
using Labor.Models.Entities.Order;

namespace Labor.DataAccess.IRepositories
{
    public interface IOrderRepository
    {
        Task<(int OrderId, string OrderNumber)> CreateOrderFromCartAsync(int employerId, int workAddressId, DateTime? scheduledDate);
        Task<OrderResponseDto?> GetOrderDetailsAsync(int orderId, int? userId = null);
        Task<IEnumerable<dynamic>> GetUserOrdersAsync(int userId, string? orderStatus = null, int pageNumber = 1, int pageSize = 20);
        Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus, string? description, int updatedBy);
        Task<bool> UpdateOrderItemStatusAsync(int orderItemId, string itemStatus, int? actualHours = null, DateTime? startTime = null, DateTime? endTime = null, int? updatedBy = null);
        Task<bool> AddOrderTrackingAsync(int orderId, string status, string? description, string? location, int? createdBy);
        Task<IEnumerable<dynamic>> GetLaborOrdersAsync(int userId, string? orderStatus = null, int pageNumber = 1, int pageSize = 20);
    }
} 