using Labor.DataAccess.IServices;

namespace Labor.API.Background;

public class LaborConfirmationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LaborConfirmationWorker> _logger;

    public LaborConfirmationWorker(IServiceProvider serviceProvider, ILogger<LaborConfirmationWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var confirmationService = scope.ServiceProvider.GetRequiredService<ILaborConfirmationService>();
                await confirmationService.ProcessPendingCallsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Labor confirmation worker failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
