using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Data;
using QDPhone.Web.Models.ViewModels;

namespace QDPhone.Web.Services;

public interface ICatalogService
{
    Task<ProductListViewModel> GetProductsAsync(
        string? keyword,
        int? categoryId,
        int? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        string? ram,
        string? rom,
        string? os,
        string? sort,
        int page,
        int pageSize);
}

public class CatalogService : ICatalogService
{
    private readonly ApplicationDbContext _db;
    public CatalogService(ApplicationDbContext db) => _db = db;

    public async Task<ProductListViewModel> GetProductsAsync(
        string? keyword,
        int? categoryId,
        int? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        string? ram,
        string? rom,
        string? os,
        string? sort,
        int page,
        int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 8, 48);

        var query = _db.Products
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(x => x.Name.Contains(keyword));
        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);
        if (brandId.HasValue)
            query = query.Where(x => x.BrandId == brandId.Value);
        if (minPrice.HasValue)
            query = query.Where(x => x.Variants.Any(v => v.Price >= minPrice.Value));
        if (maxPrice.HasValue)
            query = query.Where(x => x.Variants.Any(v => v.Price <= maxPrice.Value));
        if (!string.IsNullOrWhiteSpace(ram))
            query = query.Where(x => x.Variants.Any(v => v.Ram == ram));
        if (!string.IsNullOrWhiteSpace(rom))
            query = query.Where(x => x.Variants.Any(v => v.Rom == rom));
        if (!string.IsNullOrWhiteSpace(os))
            query = query.Where(x => x.OperatingSystem == os);

        query = sort switch
        {
            "price_asc" => query.OrderBy(x => x.Variants.Min(v => v.Price)),
            "price_desc" => query.OrderByDescending(x => x.Variants.Min(v => v.Price)),
            "latest" => query.OrderByDescending(x => x.Id),
            _ => query.OrderByDescending(x => x.Id)
        };

        var totalItems = await query.CountAsync();
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ProductListViewModel
        {
            Products = products,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            Q = keyword,
            CategoryId = categoryId,
            BrandId = brandId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Ram = ram,
            Rom = rom,
            Os = os,
            Sort = sort
        };
    }
}

