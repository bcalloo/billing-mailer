using BillingMailer.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BillingMailer.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error)> SendBillEmailAsync(string toEmail, string toName, Bill bill)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["Email:SenderName"],
                _config["Email:SenderAddress"]));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = $"Your Bill for {bill.BillingPeriodStart:MMMM yyyy}";

            var body = BuildEmailBody(toName, bill);
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["Email:SmtpHost"],
                int.Parse(_config["Email:SmtpPort"]!),
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(
                _config["Email:Username"],
                _config["Email:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            return (false, ex.Message);
        }
    }

    private static string BuildEmailBody(string name, Bill bill) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family: Arial, sans-serif; color: #333;">
          <h2>Hello, {name}!</h2>
          <p>Your bill for <strong>{bill.BillingPeriodStart:MMMM yyyy}</strong> is now available.</p>
          <table border="1" cellpadding="8" cellspacing="0" style="border-collapse:collapse; min-width:300px;">
            <tr style="background:#f5f5f5;"><td><strong>Account Number</strong></td><td>{bill.Customer.AccountNumber}</td></tr>
            <tr><td><strong>Billing Period</strong></td><td>{bill.BillingPeriodStart:MMM dd} - {bill.BillingPeriodEnd:MMM dd, yyyy}</td></tr>
            <tr style="background:#f5f5f5;"><td><strong>Amount Due</strong></td><td><strong>${bill.AmountDue:F2}</strong></td></tr>
            <tr><td><strong>Due Date</strong></td><td>{bill.DueDate:MMMM dd, yyyy}</td></tr>
          </table>
          <p style="margin-top:20px;">Thank you for your business!</p>
        </body>
        </html>
        """;
}
