using Labor.Models.DTOs.Order;

namespace Labor.DataAccess.IServices
{
    public interface ILaborConfirmationService
    {
        Task EnqueueConfirmationsForOrderAsync(int orderId);
        Task<LaborIvrProcessResult> ProcessIvrDigitAsync(int laborConfirmationId, string digit, int? updatedBy = null);
        Task<LaborIvrProcessResult> ProcessAppConfirmationAsync(int orderItemId, int laborUserId, bool accepted, int? updatedBy = null);
        Task ProcessPendingCallsAsync(CancellationToken ct = default);
        Task<OrderLaborSummaryDto?> GetOrderLaborSummaryAsync(int orderId, int? userId);
    }
}