namespace QDPhone.Web.Models.ViewModels;

public class AdminOrderIndexViewModel
{
    public List<AdminOrderRowViewModel> Rows { get; set; } = new();
    public string? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Q { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalItems { get; set; }
    public int TotalPages => TotalItems <= 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);
    public AdminOrderStatusCountViewModel StatusCounts { get; set; } = new();
}

public class AdminOrderStatusCountViewModel
{
    public int All { get; set; }
    public int Pending { get; set; }
    public int Processing { get; set; }
    public int Shipping { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
}

public class AdminOrderRowViewModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string FirstProductName { get; set; } = string.Empty;
    public int MoreProductCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? CouponCode { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
