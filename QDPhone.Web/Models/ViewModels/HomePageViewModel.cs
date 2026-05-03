using QDPhone.Web.Models.Entities;

namespace QDPhone.Web.Models.ViewModels;

public class HomePageViewModel
{
    public List<Banner> Banners { get; set; } = new();
    public List<Product> FeaturedProducts { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Product> FlashSaleProducts { get; set; } = new();
}
