namespace QDPhone.Web.Models.ViewModels;

public class AdminUserIndexViewModel
{
    public List<AdminUserRowViewModel> Rows { get; set; } = new();
    public string? Keyword { get; set; }
    public string? Role { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalItems { get; set; }
    public int TotalPages => TotalItems <= 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}
