using Labor.Models.DTOs.Payment;

namespace Labor.DataAccess.IRepositories;

public interface IPaymentRepository
{
    Task<PaymentSummaryDto?> GetPaymentSummaryAsync(int orderId, int employerId);
    Task<int> CreatePaymentRecordAsync(PaymentAmountBreakdown breakdown, int orderId, int employerId, string provider, string providerOrderId);
    Task<PaymentCompleteResultDto?> CompletePaymentAsync(int paymentId, int employerId, string providerPaymentId, string? providerSignature);
    Task<EmployerContactDto?> GetEmployerContactForOrderAsync(int orderId);
    Task InsertNotificationLogAsync(int? userId, string mobile, string channel, string templateKey, string messageBody, string status);
}
