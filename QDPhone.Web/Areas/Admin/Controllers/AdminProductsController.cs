using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Data;
using QDPhone.Web.Models.Entities;
using QDPhone.Web.Models.ViewModels;

namespace QDPhone.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "StaffOrAdmin")]
[Route("admin/products")]
public class AdminProductsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminProductsController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, int? categoryId, int? brandId, string? os, string? stock, string? sort)
    {
        var query = _db.Products
            .Include(x => x.Brand)
            .Include(x => x.Category)
            .Include(x => x.Variants)
            .Include(x => x.Images)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(kw) ||
                (x.Slug != null && x.Slug.ToLower().Contains(kw)));
        }

        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);
        if (brandId.HasValue)
            query = query.Where(x => x.BrandId == brandId.Value);
        if (!string.IsNullOrWhiteSpace(os))
            query = query.Where(x => x.OperatingSystem == os);

        query = stock switch
        {
            "low" => query.Where(x => x.Variants.Sum(v => v.StockQuantity) > 0 && x.Variants.Sum(v => v.StockQuantity) < 5),
            "out" => query.Where(x => x.Variants.Sum(v => v.StockQuantity) <= 0),
            "in" => query.Where(x => x.Variants.Sum(v => v.StockQuantity) >= 5),
            _ => query
        };

        query = sort switch
        {
            "name_asc" => query.OrderBy(x => x.Name),
            "name_desc" => query.OrderByDescending(x => x.Name),
            "stock_asc" => query.OrderBy(x => x.Variants.Sum(v => v.StockQuantity)),
            "stock_desc" => query.OrderByDescending(x => x.Variants.Sum(v => v.StockQuantity)),
            _ => query.OrderByDescending(x => x.Id)
        };

        ViewBag.Keyword = q;
        ViewBag.CategoryId = categoryId;
        ViewBag.BrandId = brandId;
        ViewBag.Os = os;
        ViewBag.Stock = stock;
        ViewBag.Sort = sort;
        ViewBag.Categories = await _db.Categories.OrderBy(x => x.Name).ToListAsync();
        ViewBag.Brands = await _db.Brands.OrderBy(x => x.Name).ToListAsync();
        ViewBag.OsList = await _db.Products
            .Where(x => !string.IsNullOrWhiteSpace(x.OperatingSystem))
            .Select(x => x.OperatingSystem)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        return View(await query.ToListAsync());
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        var vm = new ProductCreateViewModel
        {
            Brands = await _db.Brands.OrderBy(x => x.Name).ToListAsync(),
            Categories = await _db.Categories.OrderBy(x => x.Name).ToListAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> Create(ProductCreateViewModel model, IFormFile? primaryImageFile)
    {
        model.Brands = await _db.Brands.OrderBy(x => x.Name).ToListAsync();
        model.Categories = await _db.Categories.OrderBy(x => x.Name).ToListAsync();
        if (!ModelState.IsValid) return View(model);
        var product = new Product
        {
            Name = model.Name,
            Slug = model.Slug,
            Description = model.Description,
            BrandId = model.BrandId,
            CategoryId = model.CategoryId,
            OperatingSystem = model.OperatingSystem
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        _db.ProductVariants.Add(new ProductVariant
        {
            ProductId = product.Id,
            Ram = model.Ram,
            Rom = model.Rom,
            Color = model.Color,
            Price = model.Price,
            StockQuantity = model.StockQuantity
        });
        if (primaryImageFile != null && primaryImageFile.Length > 0)
            model.PrimaryImageUrl = await SaveUploadAsync(primaryImageFile, "products");
        if (!string.IsNullOrWhiteSpace(model.PrimaryImageUrl))
        {
            _db.ProductImages.Add(new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = model.PrimaryImageUrl.Trim(),
                IsPrimary = true,
                SortOrder = 0
            });
        }
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products
            .Include(x => x.Variants)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (product == null) return NotFound();
        var variant = product.Variants.OrderByDescending(x => x.Price).FirstOrDefault();
        var vm = new ProductCreateViewModel
        {
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            OperatingSystem = product.OperatingSystem,
            BrandId = product.BrandId,
            CategoryId = product.CategoryId,
            Ram = variant?.Ram ?? "8GB",
            Rom = variant?.Rom ?? "128GB",
            Color = variant?.Color ?? "Black",
            Price = variant?.Price ?? 0m,
            StockQuantity = variant?.StockQuantity ?? 0,
            PrimaryImageUrl = product.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault(),
            Brands = await _db.Brands.OrderBy(x => x.Name).ToListAsync(),
            Categories = await _db.Categories.OrderBy(x => x.Name).ToListAsync()
        };
        return View(vm);
    }

    [HttpPost("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, ProductCreateViewModel model, IFormFile? primaryImageFile)
    {
        model.Brands = await _db.Brands.OrderBy(x => x.Name).ToListAsync();
        model.Categories = await _db.Categories.OrderBy(x => x.Name).ToListAsync();
        if (!ModelState.IsValid) return View(model);

        var product = await _db.Products
            .Include(x => x.Variants)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (product == null) return NotFound();

        product.Name = model.Name;
        product.Slug = model.Slug;
        product.Description = model.Description;
        product.BrandId = model.BrandId;
        product.CategoryId = model.CategoryId;
        product.OperatingSystem = model.OperatingSystem;

        var variant = product.Variants.OrderByDescending(x => x.Price).FirstOrDefault();
        if (variant == null)
        {
            variant = new ProductVariant { ProductId = product.Id };
            _db.ProductVariants.Add(variant);
        }
        variant.Ram = model.Ram;
        variant.Rom = model.Rom;
        variant.Color = model.Color;
        variant.Price = model.Price;
        variant.StockQuantity = model.StockQuantity;

        if (primaryImageFile != null && primaryImageFile.Length > 0)
            model.PrimaryImageUrl = await SaveUploadAsync(primaryImageFile, "products");
        if (!string.IsNullOrWhiteSpace(model.PrimaryImageUrl))
        {
            var allImages = product.Images.OrderBy(x => x.SortOrder).ToList();
            foreach (var image in allImages) image.IsPrimary = false;
            var currentPrimary = allImages.FirstOrDefault();
            if (currentPrimary == null)
            {
                _db.ProductImages.Add(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = model.PrimaryImageUrl.Trim(),
                    IsPrimary = true,
                    SortOrder = 0
                });
            }
            else
            {
                currentPrimary.ImageUrl = model.PrimaryImageUrl.Trim();
                currentPrimary.IsPrimary = true;
            }
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products
            .Include(x => x.Variants)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (product == null) return RedirectToAction(nameof(Index));

        var variantIds = product.Variants.Select(v => v.Id).ToList();
        if (variantIds.Any())
        {
            var hasOrderHistory = await _db.OrderItems.AnyAsync(x => variantIds.Contains(x.ProductVariantId));
            if (hasOrderHistory)
            {
                TempData["Message"] = $"Sản phẩm \"{product.Name}\" đã có lịch sử đơn hàng nên không thể xóa.";
                return RedirectToAction(nameof(Index));
            }
        }

        _db.ProductVariants.RemoveRange(product.Variants);
        _db.ProductImages.RemoveRange(product.Images);
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{productId:int}/images")]
    public async Task<IActionResult> Images(int productId)
    {
        var product = await _db.Products
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == productId);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost]
    [Route("{productId:int}/images/add")]
    public async Task<IActionResult> AddImage(int productId, string? imageUrl, IFormFile? imageFile)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) && imageFile == null)
            return RedirectToAction(nameof(Images), new { productId });

        var product = await _db.Products.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == productId);
        if (product == null) return NotFound();

        if (imageFile != null && imageFile.Length > 0)
            imageUrl = await SaveUploadAsync(imageFile, "products");

        var nextSort = product.Images.Any() ? product.Images.Max(x => x.SortOrder) + 1 : 0;
        _db.ProductImages.Add(new ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl!.Trim(),
            IsPrimary = !product.Images.Any(),
            SortOrder = nextSort
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Images), new { productId });
    }

    [HttpPost]
    [Route("{productId:int}/images/primary")]
    public async Task<IActionResult> SetPrimaryImage(int productId, int imageId)
    {
        var images = await _db.ProductImages.Where(x => x.ProductId == productId).ToListAsync();
        if (!images.Any()) return RedirectToAction(nameof(Images), new { productId });

        foreach (var image in images)
            image.IsPrimary = image.Id == imageId;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Images), new { productId });
    }

    [HttpPost]
    [Route("{productId:int}/images/sort")]
    public async Task<IActionResult> UpdateSortOrder(int productId, int imageId, int sortOrder)
    {
        var image = await _db.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId);
        if (image == null) return RedirectToAction(nameof(Images), new { productId });
        image.SortOrder = Math.Max(0, sortOrder);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Images), new { productId });
    }

    [HttpPost]
    [Route("{productId:int}/images/delete")]
    public async Task<IActionResult> DeleteImage(int productId, int imageId)
    {
        var image = await _db.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId);
        if (image == null) return RedirectToAction(nameof(Images), new { productId });

        _db.ProductImages.Remove(image);
        await _db.SaveChangesAsync();

        var remaining = await _db.ProductImages.Where(x => x.ProductId == productId).OrderBy(x => x.SortOrder).ToListAsync();
        if (remaining.Any() && !remaining.Any(x => x.IsPrimary))
            remaining[0].IsPrimary = true;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Images), new { productId });
    }

    [HttpGet("{productId:int}/variants")]
    public async Task<IActionResult> Variants(int productId)
    {
        var product = await _db.Products.Include(x => x.Variants).FirstOrDefaultAsync(x => x.Id == productId);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost("{productId:int}/variants/add")]
    public async Task<IActionResult> AddVariant(int productId, string ram, string rom, string color, decimal price, int stockQuantity)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null) return NotFound();
        _db.ProductVariants.Add(new ProductVariant
        {
            ProductId = productId,
            Ram = ram.Trim(),
            Rom = rom.Trim(),
            Color = color.Trim(),
            Price = Math.Max(0, price),
            StockQuantity = Math.Max(0, stockQuantity)
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Variants), new { productId });
    }

    [HttpPost("{productId:int}/variants/update")]
    public async Task<IActionResult> UpdateVariant(int productId, int variantId, string ram, string rom, string color, decimal price, int stockQuantity)
    {
        var variant = await _db.ProductVariants.FirstOrDefaultAsync(x => x.Id == variantId && x.ProductId == productId);
        if (variant == null) return RedirectToAction(nameof(Variants), new { productId });
        variant.Ram = ram.Trim();
        variant.Rom = rom.Trim();
        variant.Color = color.Trim();
        variant.Price = Math.Max(0, price);
        variant.StockQuantity = Math.Max(0, stockQuantity);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Variants), new { productId });
    }

    [HttpPost("{productId:int}/variants/delete")]
    public async Task<IActionResult> DeleteVariant(int productId, int variantId)
    {
        var variantsCount = await _db.ProductVariants.CountAsync(x => x.ProductId == productId);
        if (variantsCount <= 1) return RedirectToAction(nameof(Variants), new { productId });
        var variant = await _db.ProductVariants.FirstOrDefaultAsync(x => x.Id == variantId && x.ProductId == productId);
        if (variant == null) return RedirectToAction(nameof(Variants), new { productId });
        _db.ProductVariants.Remove(variant);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Variants), new { productId });
    }

    private async Task<string> SaveUploadAsync(IFormFile file, string folderName)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        if (!allowed.Contains(ext)) throw new InvalidOperationException("Định dạng ảnh không hợp lệ.");
        var uploadRoot = Path.Combine(_env.WebRootPath, "uploads", folderName);
        Directory.CreateDirectory(uploadRoot);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadRoot, fileName);
        await using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream);
        return $"/uploads/{folderName}/{fileName}";
    }
}

