using Labor.Models.DTOs.Payment;

namespace Labor.DataAccess.IServices;

public interface IPaymentService
{
    Task<PaymentSummaryDto> GetSummaryAsync(int orderId, int employerId);
    Task<CreatePaymentResponseDto> CreatePaymentAsync(int orderId, int employerId, CreatePaymentRequestDto? request = null);
    Task<PaymentCompleteResultDto> VerifyAndCompleteAsync(int employerId, VerifyPaymentRequestDto request);
}
