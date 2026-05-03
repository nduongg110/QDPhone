using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QDPhone.Web.Data;
using QDPhone.Web.Models;
using QDPhone.Web.Models.ViewModels;

namespace QDPhone.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public HomeController(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["MetaDescription"] = "QDPhone - Mua điện thoại chính hãng, giá tốt, nhiều ưu đãi.";
        var vm = await _cache.GetOrCreateAsync("home-page-vm", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return new HomePageViewModel
            {
                Banners = await _db.Banners.Where(x => x.IsActive).OrderByDescending(x => x.Id).Take(5).ToListAsync(),
                Categories = await _db.Categories.OrderBy(x => x.Name).ToListAsync(),
                FlashSaleProducts = await _db.Products
                    .Include(x => x.Variants)
                    .Include(x => x.Images)
                    .Where(x => x.IsFeatured)
                    .OrderBy(x => x.Variants.Min(v => v.Price))
                    .Take(10)
                    .AsSplitQuery() 
                    .ToListAsync(),

                                FeaturedProducts = await _db.Products
                    .Include(x => x.Variants)
                    .Include(x => x.Images)
                    .Where(x => x.IsFeatured)
                    .OrderBy(x => x.Id) 
                    .Take(8)
                    .AsSplitQuery() 
                    .ToListAsync()
            };
        });
        return View(vm);
    }

    [HttpGet("/sitemap.xml")]
    public async Task<IActionResult> Sitemap()
    {
        var products = await _db.Products.Select(x => x.Id).ToListAsync();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var urls = new List<string>
        {
            $"{baseUrl}/",
            $"{baseUrl}/Products"
        };
        urls.AddRange(products.Select(id => $"{baseUrl}/Products/Details/{id}"));
        var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                  "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">" +
                  string.Join("", urls.Select(u => $"<url><loc>{u}</loc></url>")) +
                  "</urlset>";
        return Content(xml, "application/xml");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
