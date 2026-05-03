using QDPhone.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace QDPhone.Web.Models.ViewModels;

public class ProductCreateViewModel
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required] public string OperatingSystem { get; set; } = "Android";
    [Required] public int BrandId { get; set; }
    [Required] public int CategoryId { get; set; }
    [Required] public string Ram { get; set; } = "8GB";
    [Required] public string Rom { get; set; } = "128GB";
    [Required] public string Color { get; set; } = "Black";
    [Range(1, 1000000000)] public decimal Price { get; set; }
    [Range(0, 1000000)] public int StockQuantity { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public List<Brand> Brands { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
}
