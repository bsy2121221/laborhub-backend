using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Payment;

public class PaymentSummaryDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public bool CanPay { get; set; }
    public int PayableItemCount { get; set; }
    public int CompletedItemCount { get; set; }
    public decimal LaborAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? CouponCode { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "INR";
    public int? LastPaymentId { get; set; }
    public string? LastPaymentStatus { get; set; }
    public string? LastProviderOrderId { get; set; }
    public DateTime? LastPaidAt { get; set; }
}

public class CreatePaymentRequestDto
{
  /// <summary>Future: discount coupon code.</summary>
  public string? CouponCode { get; set; }
}

public class CreatePaymentResponseDto
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public string Provider { get; set; } = "Mock";
    public string GatewayOrderId { get; set; } = string.Empty;
    public string? RazorpayKeyId { get; set; }
    public int AmountPaise { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal LaborAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public string Currency { get; set; } = "INR";
    public string? EmployerName { get; set; }
    public string? EmployerEmail { get; set; }
    public string? EmployerContact { get; set; }
    public bool IsMock { get; set; }
}

public class VerifyPaymentRequestDto
{
    [Required]
    public int PaymentId { get; set; }

    [Required]
    public string RazorpayOrderId { get; set; } = string.Empty;

    [Required]
    public string RazorpayPaymentId { get; set; } = string.Empty;

    public string? RazorpaySignature { get; set; }
}

public class PaymentCompleteResultDto
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
}

public class EmployerContactDto
{
    public int UserId { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class PaymentAmountBreakdown
{
    public decimal LaborAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? CouponCode { get; set; }
    public decimal TotalAmount { get; set; }
}

public class GatewayOrderResult
{
    public string Provider { get; set; } = "Mock";
    public string ProviderOrderId { get; set; } = string.Empty;
    public int AmountPaise { get; set; }
}
