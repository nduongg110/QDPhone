using QDPhone.Web.Models.Entities;

namespace QDPhone.Web.Models.ViewModels;

public class AdminBannerIndexViewModel
{
    public List<Banner> Rows { get; set; } = new();
    public string? Q { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalItems { get; set; }
    public int TotalPages => TotalItems <= 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}

public class AdminReviewModerationIndexViewModel
{
    public List<Review> Rows { get; set; } = new();
    public string? Q { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalItems { get; set; }
    public int TotalPages => TotalItems <= 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}
