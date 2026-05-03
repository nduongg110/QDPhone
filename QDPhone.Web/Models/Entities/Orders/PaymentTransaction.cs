namespace QDPhone.Web.Models.Entities;

public class PaymentTransaction
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Gateway { get; set; } = "PayOS";
    public string ExternalTransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

