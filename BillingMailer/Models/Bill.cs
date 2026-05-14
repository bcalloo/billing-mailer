namespace BillingMailer.Models;

public class Bill
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public decimal AmountDue { get; set; }
    public DateTime BillingPeriodStart { get; set; }
    public DateTime BillingPeriodEnd { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsPaid { get; set; } = false;
    public ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>();
}
