using Labor.Models.DTOs.Order;

namespace Labor.DataAccess.IRepositories
{
    public interface ILaborConfirmationRepository
    {
        Task EnqueueForOrderAsync(int orderId);
        Task<LaborIvrProcessResult> ProcessIvrDigitAsync(int laborConfirmationId, string digit, int? updatedBy);
        Task<int?> GetPendingLaborConfirmationIdAsync(int orderItemId, int laborUserId);
        Task<IEnumerable<dynamic>> GetPendingForCallAsync();
        Task MarkCallAttemptAsync(int laborConfirmationId, string? providerCallId);
        Task<OrderLaborSummaryDto?> GetOrderLaborSummaryAsync(int orderId, int? userId);
        Task<(int Total, int Confirmed, int Pending, int Declined)> GetOrderCountsAsync(int orderId);
    }
}
