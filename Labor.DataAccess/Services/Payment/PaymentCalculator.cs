using Labor.Models.DTOs.Payment;
using Microsoft.Extensions.Options;

namespace Labor.DataAccess.Services.Payment;

/// <summary>
/// Central place for fee / coupon math so future platform fee and coupons plug in here.
/// </summary>
public class PaymentCalculator
{
    private readonly PaymentOptions _options;

    public PaymentCalculator(IOptions<PaymentOptions> options)
    {
        _options = options.Value;
    }

    public PaymentAmountBreakdown Calculate(decimal laborAmount, string? couponCode = null, decimal discountAmount = 0)
    {
        var platformFee = _options.PlatformFeePercent > 0
            ? Math.Round(laborAmount * _options.PlatformFeePercent / 100m, 2, MidpointRounding.AwayFromZero)
            : 0m;

        // Future: resolve couponCode to discountAmount via coupon service
        var total = Math.Max(0, laborAmount + platformFee - discountAmount);

        return new PaymentAmountBreakdown
        {
            LaborAmount = laborAmount,
            PlatformFee = platformFee,
            DiscountAmount = discountAmount,
            CouponCode = string.IsNullOrWhiteSpace(couponCode) ? null : couponCode.Trim(),
            TotalAmount = total
        };
    }
}
