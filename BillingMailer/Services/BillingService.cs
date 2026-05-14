using BillingMailer.Data;
using BillingMailer.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingMailer.Services;

public class BillingService
{
    private readonly AppDbContext _db;
    private readonly EmailService _emailService;
    private readonly ILogger<BillingService> _logger;

    public BillingService(AppDbContext db, EmailService emailService, ILogger<BillingService> logger)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendMonthlyBillsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var periodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var bills = await _db.Bills
            .Include(b => b.Customer)
            .Include(b => b.EmailLogs)
            .Where(b =>
                b.BillingPeriodStart == periodStart &&
                b.Customer.IsActive &&
                !b.EmailLogs.Any(e => e.Success && e.SentAt >= periodStart))
            .ToListAsync(ct);

        _logger.LogInformation("Found {Count} bill(s) to send for {Period}",
            bills.Count, periodStart.ToString("MMMM yyyy"));

        int sent = 0, failed = 0;

        foreach (var bill in bills)
        {
            var (success, error) = await _emailService.SendBillEmailAsync(
                bill.Customer.Email, bill.Customer.FullName, bill);

            _db.EmailLogs.Add(new EmailLog
            {
                BillId = bill.Id,
                SentAt = DateTime.UtcNow,
                Success = success,
                ErrorMessage = error
            });

            if (success) sent++;
            else failed++;

            _logger.LogInformation("Bill {BillId} to {Email}: {Status}",
                bill.Id, bill.Customer.Email, success ? "Sent" : $"Failed - {error}");
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Monthly billing complete. Sent: {Sent}, Failed: {Failed}", sent, failed);
    }
}
