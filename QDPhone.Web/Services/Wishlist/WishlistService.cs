using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Data;
using QDPhone.Web.Models.Entities;

namespace QDPhone.Web.Services;

public interface IWishlistService
{
    Task AddAsync(string userId, int productId);
    Task RemoveAsync(string userId, int productId);
    Task<List<Product>> GetItemsAsync(string userId);
}

public class WishlistService : IWishlistService
{
    private readonly ApplicationDbContext _db;
    public WishlistService(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(string userId, int productId)
    {
        var exists = await _db.WishlistItems.AnyAsync(x => x.UserId == userId && x.ProductId == productId);
        if (exists) return;
        _db.WishlistItems.Add(new WishlistItem { UserId = userId, ProductId = productId });
        await _db.SaveChangesAsync();
    }

    public async Task RemoveAsync(string userId, int productId)
    {
        var item = await _db.WishlistItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
        if (item == null) return;
        _db.WishlistItems.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Product>> GetItemsAsync(string userId)
    {
        var productIds = await _db.WishlistItems.Where(x => x.UserId == userId).Select(x => x.ProductId).ToListAsync();
        return await _db.Products
            .Include(x => x.Variants)
            .Include(x => x.Images)
            .Where(x => productIds.Contains(x.Id))
            .ToListAsync();
    }
}

