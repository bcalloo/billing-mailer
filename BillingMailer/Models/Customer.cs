namespace BillingMailer.Models;

public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
