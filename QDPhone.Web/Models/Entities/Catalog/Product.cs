namespace QDPhone.Web.Models.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OperatingSystem { get; set; } = "Unknown";
    public bool IsFeatured { get; set; }
    public int BrandId { get; set; }
    public int CategoryId { get; set; }
    public Brand? Brand { get; set; }
    public Category? Category { get; set; }
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}

