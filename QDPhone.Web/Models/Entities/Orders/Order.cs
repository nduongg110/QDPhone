namespace QDPhone.Web.Models.Entities;

public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string PaymentMethod { get; set; } = "COD";
    public int? CouponId { get; set; }
    public string? CouponCode { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

