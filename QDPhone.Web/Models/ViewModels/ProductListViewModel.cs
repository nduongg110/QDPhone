using QDPhone.Web.Models.Entities;

namespace QDPhone.Web.Models.ViewModels;

public class ProductListViewModel
{
    public List<Product> Products { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalItems { get; set; }
    public int TotalPages => TotalItems <= 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public string? Q { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Ram { get; set; }
    public string? Rom { get; set; }
    public string? Os { get; set; }
    public string? Sort { get; set; }
}
