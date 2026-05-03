using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Data;
using QDPhone.Web.Models.Entities;
using QDPhone.Web.Models.ViewModels;

namespace QDPhone.Web.Services;

public interface ICartService
{
    Task<Cart> GetCartAsync(string userId);
    Task<CartViewModel> GetCartViewAsync(string userId);
    Task<int> GetCartItemCountAsync(string userId);
    Task AddItemAsync(string userId, int variantId, int quantity);
    Task UpdateItemAsync(string userId, int cartItemId, int quantity);
    Task RemoveItemAsync(string userId, int cartItemId);
    Task ClearCartAsync(string userId);
}

public class CartService : ICartService
{
    private readonly ApplicationDbContext _db;
    public CartService(ApplicationDbContext db) => _db = db;

    public async Task<Cart> GetCartAsync(string userId)
    {
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(x => x.UserId == userId);
        if (cart != null) return cart;
        cart = new Cart { UserId = userId };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
        return cart;
    }

    public async Task<CartViewModel> GetCartViewAsync(string userId)
    {
        var cart = await GetCartAsync(userId);
        var variantIds = cart.Items.Select(x => x.ProductVariantId).Distinct().ToList();
        var variants = await _db.ProductVariants
            .Include(v => v.Product)
            .Where(v => variantIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id);
        var result = new CartViewModel();
        foreach (var item in cart.Items)
        {
            if (!variants.TryGetValue(item.ProductVariantId, out var variant)) continue;
            result.Items.Add(new CartItemViewModel
            {
                CartItemId = item.Id,
                VariantId = variant.Id,
                ProductName = variant.Product?.Name ?? "Unknown",
                VariantLabel = $"{variant.Ram}/{variant.Rom}/{variant.Color}",
                Quantity = item.Quantity,
                UnitPrice = variant.Price
            });
        }

        return result;
    }

    public async Task<int> GetCartItemCountAsync(string userId)
    {
        var cart = await GetCartAsync(userId);
        return cart.Items.Sum(x => x.Quantity);
    }

    public async Task AddItemAsync(string userId, int variantId, int quantity)
    {
        if (quantity <= 0) quantity = 1;
        var variant = await _db.ProductVariants.FindAsync(variantId);
        if (variant == null || variant.StockQuantity <= 0)
            throw new InvalidOperationException("Sản phẩm đã hết hàng.");
        var cart = await GetCartAsync(userId);
        var existing = cart.Items.FirstOrDefault(x => x.ProductVariantId == variantId);
        if (existing == null)
        {
            if (quantity > variant.StockQuantity)
                throw new InvalidOperationException($"Chỉ còn {variant.StockQuantity} sản phẩm trong kho.");
            _db.CartItems.Add(new CartItem { CartId = cart.Id, ProductVariantId = variantId, Quantity = quantity });
        }
        else
        {
            var nextQuantity = existing.Quantity + quantity;
            if (nextQuantity > variant.StockQuantity)
                throw new InvalidOperationException($"Chỉ còn {variant.StockQuantity} sản phẩm trong kho.");
            existing.Quantity = nextQuantity;
        }

        await _db.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(string userId, int cartItemId, int quantity)
    {
        var cart = await GetCartAsync(userId);
        var item = cart.Items.FirstOrDefault(x => x.Id == cartItemId);
        if (item == null) return;
        if (quantity <= 0)
            _db.CartItems.Remove(item);
        else
        {
            var variant = await _db.ProductVariants.FindAsync(item.ProductVariantId);
            if (variant == null || quantity > variant.StockQuantity)
                throw new InvalidOperationException($"Chỉ còn {variant?.StockQuantity ?? 0} sản phẩm trong kho.");
            item.Quantity = quantity;
        }
        await _db.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(string userId, int cartItemId)
    {
        var cart = await GetCartAsync(userId);
        var item = cart.Items.FirstOrDefault(x => x.Id == cartItemId);
        if (item == null) return;
        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task ClearCartAsync(string userId)
    {
        var cart = await GetCartAsync(userId);
        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync();
    }
}

