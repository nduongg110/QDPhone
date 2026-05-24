using QDPhone.Web.Data;
using QDPhone.Web.Models.Entities;

namespace QDPhone.Web.Services;

public interface IInventoryService
{
    Task<bool> TryReserveAsync(int variantId, int quantity);
}

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _db;
    public InventoryService(ApplicationDbContext db) => _db = db;

    public async Task<bool> TryReserveAsync(int variantId, int quantity)
    {
        var variant = await _db.ProductVariants.FindAsync(variantId);
        if (variant == null || variant.StockQuantity < quantity) return false;
        variant.StockQuantity -= quantity;
        _db.InventoryTransactions.Add(new InventoryTransaction
        {
            ProductVariantId = variantId,
            DeltaQuantity = -quantity,
            Reason = "OrderPlacement"
        });
        await _db.SaveChangesAsync();
        return true;
    }
}

