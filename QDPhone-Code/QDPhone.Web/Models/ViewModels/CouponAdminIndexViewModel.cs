namespace QDPhone.Web.Models.ViewModels;

public class CouponAdminIndexViewModel
{
    public List<CouponAdminRowViewModel> Rows { get; set; } = new();
    public string? Status { get; set; }
    public string? Keyword { get; set; }
    public DateTime? ExpireFrom { get; set; }
    public DateTime? ExpireTo { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalItems { get; set; }
    public int TotalPages => TotalItems <= 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}
