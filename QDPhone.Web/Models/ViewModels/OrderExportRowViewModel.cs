namespace QDPhone.Web.Models.ViewModels;

public class OrderExportRowViewModel
{
    public int OrderId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
