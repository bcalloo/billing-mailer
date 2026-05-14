using BillingMailer.Services;

namespace BillingMailer;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<Worker> _logger;

    public Worker(IServiceProvider services, ILogger<Worker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Billing Mailer Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var nextRun = GetNextRunTime();
            var delay = nextRun - DateTime.Now;

            _logger.LogInformation("Next billing run scheduled for: {NextRun}", nextRun);

            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                using var scope = _services.CreateScope();
                var billingService = scope.ServiceProvider.GetRequiredService<BillingService>();
                await billingService.SendMonthlyBillsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the monthly billing run.");
            }
        }

        _logger.LogInformation("Billing Mailer Worker stopped.");
    }

    private static DateTime GetNextRunTime()
    {
        var now = DateTime.Now;
        var candidate = new DateTime(now.Year, now.Month, 1, 8, 0, 0);

        if (now >= candidate)
            candidate = candidate.AddMonths(1);

        return candidate;
    }
}
