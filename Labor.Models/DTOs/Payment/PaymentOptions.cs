namespace Labor.Models.DTOs.Payment;

public class PaymentOptions
{
    public const string SectionName = "Payment";

    /// <summary>Mock or Razorpay</summary>
    public string Provider { get; set; } = "Mock";

    /// <summary>Future: platform fee as percent of labor amount (0 = disabled).</summary>
    public decimal PlatformFeePercent { get; set; } = 0;
}

public class RazorpayOptions
{
    public const string SectionName = "Razorpay";

    public string KeyId { get; set; } = string.Empty;
    public string KeySecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}
