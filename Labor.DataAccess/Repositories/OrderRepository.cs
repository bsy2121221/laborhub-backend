using Dapper;
using Labor.DataAccess.Context;
using Labor.DataAccess.IRepositories;
using Labor.Models.DTOs.Order;
using System.Data;
using System.Runtime.InteropServices;

namespace Labor.DataAccess.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbContext _context;

        public OrderRepository(IDbContext context)
        {
            _context = context;
        }

        public async Task<(int OrderId, string OrderNumber)> CreateOrderFromCartAsync(int employerId, int workAddressId, DateTime? scheduledDate)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@EmployerId", employerId);
            parameters.Add("@WorkAddressId", workAddressId);
            parameters.Add("@ScheduledDate", scheduledDate);

            var result = await connection.QuerySingleAsync<CreateOrderFromCartResult>(
                "[dbo].[sp_CreateOrderFromCart]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return (result.OrderID, result.OrderNumber);
        }

        public async Task<OrderResponseDto?> GetOrderDetailsAsync(int orderId, int? userId = null)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", orderId);
            parameters.Add("@UserId", userId);

            using var multi = await connection.QueryMultipleAsync(
                "[dbo].[sp_GetOrderDetails]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var orderInfo = await multi.ReadFirstOrDefaultAsync<dynamic>();
            if (orderInfo == null) return null;

            var orderItems = (await multi.ReadAsync<dynamic>()).Select(item => new OrderItemDto
            {
                OrderItemId = item.OrderItemId,
                LaborId = item.LaborId,
                RequiredHours = item.RequiredHours,
                DailyRate = item.DailyRate,
                TotalAmount = item.TotalAmount,
                WorkDescription = item.WorkDescription,
                ItemStatus = item.ItemStatus,
                ActualHours = item.ActualHours,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
                LaborName = item.LaborName,
                LaborMobile = item.LaborMobile,
                ProfilePicture = item.ProfilePicture,
                LaborType = item.LaborType,
                Specialization = item.Specialization,
                Rating = item.Rating
            }).ToList();

            var orderTrackings = (await multi.ReadAsync<dynamic>()).Select(tracking => new OrderTrackingDto
            {
                TrackingId = tracking.TrackingId,
                Status = tracking.Status,
                Description = tracking.Description,
                Location = tracking.Location,
                CreatedAt = tracking.CreatedAt
            }).ToList();

            return new OrderResponseDto
            {
                OrderId = orderInfo.OrderId,
                OrderNumber = orderInfo.OrderNumber,
                EmployerId = orderInfo.EmployerId,
                TotalAmount = orderInfo.TotalAmount,
                OrderStatus = orderInfo.OrderStatus,
                PaymentStatus = orderInfo.PaymentStatus,
                ScheduledDate = orderInfo.ScheduledDate,
                CompletedDate = orderInfo.CompletedDate,
                CancelledDate = orderInfo.CancelledDate,
                CancelReason = orderInfo.CancelReason,
                CreatedAt = orderInfo.CreatedAt,
                EmployerName = orderInfo.EmployerName,
                EmployerMobile = orderInfo.EmployerMobile,
                WorkAddress = new AddressDto
                {
                    Street = orderInfo.Street,
                    City = orderInfo.City,
                    State = orderInfo.State,
                    Country = orderInfo.Country,
                    ZipCode = orderInfo.ZipCode
                },
                OrderItems = orderItems,
                OrderTrackings = orderTrackings
            };
        }

        public async Task<IEnumerable<dynamic>> GetUserOrdersAsync(int userId, string? orderStatus = null, int pageNumber = 1, int pageSize = 20)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@OrderStatus", orderStatus);
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);

            return await connection.QueryAsync<dynamic>(
                "[dbo].[sp_GetUserOrders]",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus, string? description, int updatedBy)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", orderId);
            parameters.Add("@NewStatus", newStatus);
            parameters.Add("@Description", description);
            parameters.Add("@UpdatedBy", updatedBy);

            var result = await connection.QuerySingleAsync<int>(
                "[dbo].[sp_UpdateOrderStatus]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result > 0;
        }

        public async Task<bool> UpdateOrderItemStatusAsync(int orderItemId, string itemStatus, int? actualHours = null, DateTime? startTime = null, DateTime? endTime = null, int? updatedBy = null)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@OrderItemID", orderItemId);
            parameters.Add("@ItemStatus", itemStatus);
            parameters.Add("@ActualHours", actualHours);
            parameters.Add("@StartTime", startTime);
            parameters.Add("@EndTime", endTime);
            parameters.Add("@UpdatedBy", updatedBy);

            var result = await connection.QuerySingleAsync<int>(
                "[dbo].[sp_UpdateOrderItemStatus]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result > 0;
        }

        public async Task<bool> AddOrderTrackingAsync(int orderId, string status, string? description, string? location, int? createdBy)
        {
            using var connection = _context.CreateConnection();
            var sql = @"
                INSERT INTO [dbo].[OrderTracking] (OrderID, Status, Description, Location, CreatedBy)
                VALUES (@OrderID, @Status, @Description, @Location, @CreatedBy)";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                OrderID = orderId,
                Status = status,
                Description = description,
                Location = location,
                CreatedBy = createdBy
            });

            return rowsAffected > 0;
        }

        public async Task<IEnumerable<dynamic>> GetLaborOrdersAsync(int userId, string? orderStatus = null, int pageNumber = 1, int pageSize = 20)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserID", userId);
            parameters.Add("@OrderStatus", orderStatus);
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);

            return await connection.QueryAsync<dynamic>(
                "[dbo].[sp_GetLaborOrders]",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
} 