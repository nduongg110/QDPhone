using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QDPhone.Web.Data;
using QDPhone.Web.Services;
using System.Security.Claims;

namespace QDPhone.Web.Controllers;

public class ProductsController : Controller
{
    private readonly ICatalogService _catalogService;
    private readonly ApplicationDbContext _db;
    private readonly ICartService _cartService;
    private readonly IMemoryCache _cache;

    public ProductsController(ICatalogService catalogService, ApplicationDbContext db, ICartService cartService, IMemoryCache cache)
    {
        _catalogService = catalogService;
        _db = db;
        _cartService = cartService;
        _cache = cache;
    }

    public async Task<IActionResult> Index(
        string? q,
        int? categoryId,
        int? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        string? ram,
        string? rom,
        string? os,
        string? sort,
        int page = 1,
        int pageSize = 12)
    {
        ViewBag.Brands = await _cache.GetOrCreateAsync("brands-list", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _db.Brands.OrderBy(x => x.Name).ToListAsync();
        });
        ViewBag.Categories = await _cache.GetOrCreateAsync("categories-list", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _db.Categories.OrderBy(x => x.Name).ToListAsync();
        });
        return View(await _catalogService.GetProductsAsync(q, categoryId, brandId, minPrice, maxPrice, ram, rom, os, sort, page, pageSize));
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products
            .Include(x => x.Brand)
            .Include(x => x.Category)
            .Include(x => x.Variants)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (product == null) return NotFound();
        ViewBag.RelatedProducts = await _db.Products
            .Include(x => x.Variants)
            .Include(x => x.Images)
            .Where(x => x.CategoryId == product.CategoryId && x.Id != id)
            .Take(4)
            .ToListAsync();
        ViewBag.Reviews = await _db.Reviews.Where(x => x.ProductId == id).OrderByDescending(x => x.Id).ToListAsync();
        return View(product);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddToCart(int variantId, int quantity = 1)
    {
        if (quantity <= 0) quantity = 1;
        var variant = await _db.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId);
        if (variant == null || variant.StockQuantity < quantity)
        {
            TempData["Message"] = "Phiên bản sản phẩm không hợp lệ hoặc đã hết hàng.";
            return RedirectToAction(nameof(Index));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await _cartService.AddItemAsync(userId, variantId, quantity);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Message"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction("Index", "Cart");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> BuyNow(int variantId, int quantity = 1)
    {
        if (quantity <= 0) quantity = 1;
        var variant = await _db.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId);
        if (variant == null || variant.StockQuantity < quantity)
        {
            TempData["Message"] = "Phiên bản sản phẩm không hợp lệ hoặc đã hết hàng.";
            return RedirectToAction(nameof(Index));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await _cartService.AddItemAsync(userId, variantId, quantity);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Message"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction("Index", "Checkout");
    }
}

