using Labor.DataAccess.IRepositories;
using Labor.DataAccess.IServices;
using Labor.Models.DTOs.Order;
using Labor.Models.DTOs.TelePhony;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Labor.DataAccess.Services;

public class LaborConfirmationService : ILaborConfirmationService
{
    private readonly ILaborConfirmationRepository _repository;
    private readonly IIvrService _ivrService;
    private readonly INotificationService _notificationService;
    private readonly TelephonyOptions _telephonyOptions;
    private readonly ILogger<LaborConfirmationService> _logger;

    public LaborConfirmationService(
        ILaborConfirmationRepository repository,
        IIvrService ivrService,
        INotificationService notificationService,
        IOptions<TelephonyOptions> telephonyOptions,
        ILogger<LaborConfirmationService> logger)
    {
        _repository = repository;
        _ivrService = ivrService;
        _notificationService = notificationService;
        _telephonyOptions = telephonyOptions.Value;
        _logger = logger;
    }

    public async Task EnqueueConfirmationsForOrderAsync(int orderId)
    {
        await _repository.EnqueueForOrderAsync(orderId);
        await ProcessPendingCallsAsync();
    }

    public async Task<LaborIvrProcessResult> ProcessIvrDigitAsync(int laborConfirmationId, string digit, int? updatedBy = null)
    {
        if (digit is not ("1" or "2"))
        {
            return new LaborIvrProcessResult { Success = false };
        }

        var result = await _repository.ProcessIvrDigitAsync(laborConfirmationId, digit, updatedBy);
        if (!result.Success || result.OrderId is null)
        {
            return result;
        }

        var orderId = result.OrderId.Value;

        if (digit == "1" && result.LaborId is not null)
        {
            await _notificationService.NotifyLaborConfirmedAsync(orderId, result.LaborId.Value);
        }
        else if (digit == "2" && result.LaborId is not null)
        {
            await _notificationService.NotifyEmployerLaborDeclinedAsync(orderId, result.LaborId.Value);
        }

        await _notificationService.NotifyEmployerLaborProgressAsync(orderId);
        return result;
    }

    public async Task<LaborIvrProcessResult> ProcessAppConfirmationAsync(int orderItemId, int laborUserId, bool accepted, int? updatedBy = null)
    {
        var confirmationId = await _repository.GetPendingLaborConfirmationIdAsync(orderItemId, laborUserId);
        if (confirmationId is null)
        {
            return new LaborIvrProcessResult { Success = false };
        }

        return await ProcessIvrDigitAsync(confirmationId.Value, accepted ? "1" : "2", updatedBy);
    }

    public async Task ProcessPendingCallsAsync(CancellationToken ct = default)
    {
        if (IsQuietHours())
        {
            _logger.LogDebug("Skipping IVR calls during quiet hours.");
            return;
        }

        var rows = await _repository.GetPendingForCallAsync();
        foreach (var row in rows)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            int confirmationId = (int)row.LaborConfirmationID;
            int attemptCount = (int)row.AttemptCount;

            if (attemptCount >= _telephonyOptions.MaxAttempts)
            {
                _logger.LogWarning(
                    "Labor confirmation {ConfirmationId} exceeded max attempts ({Max}).",
                    confirmationId,
                    _telephonyOptions.MaxAttempts);
                continue;
            }

            string mobile = row.LaborMobile;
            string employerName = row.EmployerName ?? "Employer";
            string workArea = row.WorkArea ?? "work location";
            DateTime? scheduledDate = row.ScheduledDate;

            try
            {
                var callId = await _ivrService.PlaceLaborConfirmationCallAsync(
                    confirmationId,
                    mobile,
                    employerName,
                    workArea,
                    scheduledDate,
                    ct);

                await _repository.MarkCallAttemptAsync(confirmationId, callId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to place IVR call for confirmation {ConfirmationId}", confirmationId);
            }
        }
    }

    public Task<OrderLaborSummaryDto?> GetOrderLaborSummaryAsync(int orderId, int? userId)
    {
        return _repository.GetOrderLaborSummaryAsync(orderId, userId);
    }

    private bool IsQuietHours()
    {
        var hour = DateTime.Now.Hour;
        var start = _telephonyOptions.QuietHoursStart;
        var end = _telephonyOptions.QuietHoursEnd;

        if (start < end)
        {
            return hour >= start && hour < end;
        }

        return hour >= start || hour < end;
    }
}
