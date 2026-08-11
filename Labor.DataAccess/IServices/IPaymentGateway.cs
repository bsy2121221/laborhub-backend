using Labor.Models.DTOs.Payment;

namespace Labor.DataAccess.IServices;

public interface IPaymentGateway
{
    string ProviderName { get; }
    Task<GatewayOrderResult> CreateOrderAsync(decimal totalAmountInr, string receipt, string? notes = null);
    bool VerifyPaymentSignature(string gatewayOrderId, string gatewayPaymentId, string signature);
}
