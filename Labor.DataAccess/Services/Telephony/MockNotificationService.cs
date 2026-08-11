using Labor.DataAccess.IRepositories;
using Labor.DataAccess.IServices;
using Microsoft.Extensions.Logging;

namespace Labor.DataAccess.Services.Telephony;

public class MockNotificationService : INotificationService
{
    private readonly ILaborConfirmationRepository _repo;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<MockNotificationService> _logger;

    public MockNotificationService(
        ILaborConfirmationRepository repo,
        IPaymentRepository paymentRepository,
        ILogger<MockNotificationService> logger)
    {
        _repo = repo;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task NotifyEmployerLaborProgressAsync(int orderId)
    {
        var s = await _repo.GetOrderCountsAsync(orderId);
        _logger.LogWarning("[MOCK SMS/WA] Employer order {Order}: {Confirmed}/{Total} labor confirmed, {Pending} pending, {Declined} declined.",
            orderId, s.Confirmed, s.Total, s.Pending, s.Declined);
    }

    public Task NotifyLaborConfirmedAsync(int orderId, int laborId)
    {
        _logger.LogWarning("[MOCK SMS/WA] Labor {LaborId} confirmed for order {OrderId}", laborId, orderId);
        return Task.CompletedTask;
    }

    public Task NotifyEmployerLaborDeclinedAsync(int orderId, int laborId)
    {
        _logger.LogWarning("[MOCK SMS/WA] Labor {LaborId} declined order {OrderId}", laborId, orderId);
        return Task.CompletedTask;
    }

    public async Task NotifyEmployerPaymentSuccessAsync(int orderId, string orderNumber, decimal amountPaid)
    {
        var contact = await _paymentRepository.GetEmployerContactForOrderAsync(orderId);
        var mobile = contact?.MobileNumber ?? "unknown";
        var name = contact == null ? "Employer" : $"{contact.FirstName} {contact.LastName}".Trim();
        var message =
            $"Payment of ₹{amountPaid:N0} received for order {orderNumber}. Thank you for using LaborHub.";

        _logger.LogWarning(
            "[MOCK SMS/WA] Payment success to {Name} ({Mobile}): {Message}",
            name, mobile, message);

        await _paymentRepository.InsertNotificationLogAsync(
            contact?.UserId,
            mobile,
            "SMS",
            "employer_payment_success",
            message,
            "Mocked");

        await _paymentRepository.InsertNotificationLogAsync(
            contact?.UserId,
            mobile,
            "WhatsApp",
            "employer_payment_success",
            message,
            "Mocked");
    }
}
