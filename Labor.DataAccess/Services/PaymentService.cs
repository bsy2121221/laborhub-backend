using Labor.DataAccess.IRepositories;
using Labor.DataAccess.IServices;
using Labor.DataAccess.Services.Payment;
using Labor.Models.DTOs.Payment;
using Microsoft.Extensions.Logging;

namespace Labor.DataAccess.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly PaymentCalculator _calculator;
    private readonly INotificationService _notificationService;
    private readonly RazorpayOptions _razorpayOptions;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway,
        PaymentCalculator calculator,
        INotificationService notificationService,
        Microsoft.Extensions.Options.IOptions<RazorpayOptions> razorpayOptions,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
        _calculator = calculator;
        _notificationService = notificationService;
        _razorpayOptions = razorpayOptions.Value;
        _logger = logger;
    }

    public async Task<PaymentSummaryDto> GetSummaryAsync(int orderId, int employerId)
    {
        var summary = await _paymentRepository.GetPaymentSummaryAsync(orderId, employerId)
            ?? throw new InvalidOperationException("Order not found.");

        var breakdown = _calculator.Calculate(summary.LaborAmount, summary.CouponCode, summary.DiscountAmount);
        summary.PlatformFee = breakdown.PlatformFee;
        summary.DiscountAmount = breakdown.DiscountAmount;
        summary.TotalAmount = breakdown.TotalAmount;

        return summary;
    }

    public async Task<CreatePaymentResponseDto> CreatePaymentAsync(int orderId, int employerId, CreatePaymentRequestDto? request = null)
    {
        var summary = await GetSummaryAsync(orderId, employerId);
        if (!summary.CanPay)
        {
            throw new InvalidOperationException("Payment is not available until all workers have completed their work.");
        }

        if (summary.TotalAmount <= 0)
        {
            throw new InvalidOperationException("Nothing to pay for this order.");
        }

        // Future: validate coupon and set discountAmount
        var breakdown = _calculator.Calculate(summary.LaborAmount, request?.CouponCode, discountAmount: 0);
        var receipt = $"order_{orderId}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        var gatewayOrder = await _paymentGateway.CreateOrderAsync(breakdown.TotalAmount, receipt, $"LaborHub order {summary.OrderNumber}");

        var paymentId = await _paymentRepository.CreatePaymentRecordAsync(
            breakdown,
            orderId,
            employerId,
            _paymentGateway.ProviderName,
            gatewayOrder.ProviderOrderId);

        var contact = await _paymentRepository.GetEmployerContactForOrderAsync(orderId);

        return new CreatePaymentResponseDto
        {
            PaymentId = paymentId,
            OrderId = orderId,
            Provider = _paymentGateway.ProviderName,
            GatewayOrderId = gatewayOrder.ProviderOrderId,
            RazorpayKeyId = _paymentGateway.ProviderName == "Razorpay" ? _razorpayOptions.KeyId : null,
            AmountPaise = gatewayOrder.AmountPaise,
            TotalAmount = breakdown.TotalAmount,
            LaborAmount = breakdown.LaborAmount,
            PlatformFee = breakdown.PlatformFee,
            DiscountAmount = breakdown.DiscountAmount,
            Currency = "INR",
            EmployerName = contact == null ? null : $"{contact.FirstName} {contact.LastName}".Trim(),
            EmployerContact = contact?.MobileNumber,
            IsMock = _paymentGateway.ProviderName == "Mock"
        };
    }

    public async Task<PaymentCompleteResultDto> VerifyAndCompleteAsync(int employerId, VerifyPaymentRequestDto request)
    {
        if (!_paymentGateway.VerifyPaymentSignature(
                request.RazorpayOrderId,
                request.RazorpayPaymentId,
                request.RazorpaySignature ?? string.Empty))
        {
            throw new InvalidOperationException("Payment verification failed.");
        }

        var result = await _paymentRepository.CompletePaymentAsync(
            request.PaymentId,
            employerId,
            request.RazorpayPaymentId,
            request.RazorpaySignature);

        if (result == null)
        {
            throw new InvalidOperationException("Payment not found or already completed.");
        }

        _logger.LogInformation("Payment {PaymentId} completed for order {OrderId}", result.PaymentId, result.OrderId);

        await _notificationService.NotifyEmployerPaymentSuccessAsync(
            result.OrderId,
            result.OrderNumber,
            result.AmountPaid);

        return result;
    }
}
