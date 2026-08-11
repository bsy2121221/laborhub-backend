using Labor.DataAccess.IServices;
using Labor.Models.DTOs.Payment;
using Microsoft.Extensions.Logging;

namespace Labor.DataAccess.Services.Payment;

public class MockPaymentGateway : IPaymentGateway
{
    private readonly ILogger<MockPaymentGateway> _logger;

    public MockPaymentGateway(ILogger<MockPaymentGateway> logger)
    {
        _logger = logger;
    }

    public string ProviderName => "Mock";

    public Task<GatewayOrderResult> CreateOrderAsync(decimal totalAmountInr, string receipt, string? notes = null)
    {
        var orderId = $"mock_order_{Guid.NewGuid():N}";
        _logger.LogInformation("[MOCK PAYMENT] Created gateway order {OrderId} for {Amount} INR (receipt: {Receipt})",
            orderId, totalAmountInr, receipt);

        return Task.FromResult(new GatewayOrderResult
        {
            Provider = ProviderName,
            ProviderOrderId = orderId,
            AmountPaise = (int)Math.Round(totalAmountInr * 100m, MidpointRounding.AwayFromZero)
        });
    }

    public bool VerifyPaymentSignature(string gatewayOrderId, string gatewayPaymentId, string signature)
    {
        var ok = gatewayOrderId.StartsWith("mock_order_", StringComparison.OrdinalIgnoreCase)
                 && gatewayPaymentId.StartsWith("mock_pay_", StringComparison.OrdinalIgnoreCase);

        if (!ok)
        {
            _logger.LogWarning("[MOCK PAYMENT] Signature verification failed for order {OrderId}", gatewayOrderId);
        }

        return ok;
    }
}
