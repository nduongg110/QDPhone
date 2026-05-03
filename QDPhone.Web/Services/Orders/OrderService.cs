using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Data;
using QDPhone.Web.Models.Entities;

namespace QDPhone.Web.Services;

public interface IOrderService
{
    Task<Order> PlaceCodOrderAsync(string userId, decimal totalAmount);
    Task<bool> UpdatePaymentStatusAsync(int orderId, string status);
    Task<bool> RestoreStockForFailedPaymentAsync(int orderId);
    Task<(Order? order, string? error)> PlaceOrderFromCartAsync(string userId, decimal discountAmount, string paymentMethod, int? couponId = null, string? couponCode = null);
}

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _db;
    public OrderService(ApplicationDbContext db) => _db = db;

    public async Task<Order> PlaceCodOrderAsync(string userId, decimal totalAmount)
    {
        var order = new Order { UserId = userId, TotalAmount = totalAmount, PaymentMethod = "COD", Status = "Pending" };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return order;
    }

    public async Task<bool> UpdatePaymentStatusAsync(int orderId, string status)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null) return false;
        if (!IsOrderStatusTransitionAllowed(order.Status, status)) return false;
        if (order.Status == status) return true;
        order.Status = status;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreStockForFailedPaymentAsync(int orderId)
    {
        var order = await _db.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == orderId);
        if (order == null || !order.Items.Any()) return false;

        var restoreReason = $"OrderPaymentFailedRestore:{orderId}";
        var restored = await _db.InventoryTransactions.AnyAsync(x => x.Reason == restoreReason);
        if (restored) return true;

        var variantIds = order.Items.Select(i => i.ProductVariantId).Distinct().ToList();
        var variants = await _db.ProductVariants
            .Where(v => variantIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id);

        foreach (var item in order.Items)
        {
            if (!variants.TryGetValue(item.ProductVariantId, out var variant)) continue;
            variant.StockQuantity += item.Quantity;
            _db.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductVariantId = variant.Id,
                DeltaQuantity = item.Quantity,
                Reason = restoreReason
            });
        }

        await _db.SaveChangesAsync();
        return true;
    }

    private static bool IsOrderStatusTransitionAllowed(string current, string target)
    {
        if (string.Equals(current, target, StringComparison.OrdinalIgnoreCase)) return true;
        return current switch
        {
            "Pending" => target is "PendingPayment" or "Paid" or "Cancelled" or "PaymentFailed" or "Shipping",
            "PendingPayment" => target is "Paid" or "PaymentFailed" or "Cancelled",
            "Paid" => target is "Shipping" or "Done",
            "Shipping" => target is "Done" or "Cancelled",
            "PaymentFailed" => target is "PendingPayment",
            "Cancelled" => false,
            "Done" => false,
            _ => false
        };
    }

    public async Task<(Order? order, string? error)> PlaceOrderFromCartAsync(string userId, decimal discountAmount, string paymentMethod, int? couponId = null, string? couponCode = null)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(x => x.UserId == userId);
        if (cart == null || !cart.Items.Any()) return (null, "Giỏ hàng trống.");

        var variantIds = cart.Items.Select(x => x.ProductVariantId).Distinct().ToList();
        var variants = await _db.ProductVariants
            .Include(v => v.Product)
            .Where(v => variantIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id);
        decimal subtotal = 0m;
        foreach (var item in cart.Items)
        {
            if (!variants.TryGetValue(item.ProductVariantId, out var variant))
                return (null, "Sản phẩm không tồn tại.");
            if (variant.StockQuantity < item.Quantity)
                return (null, $"Không đủ tồn kho cho biến thể {variant.Id}.");
            subtotal += variant.Price * item.Quantity;
        }

        foreach (var item in cart.Items)
        {
            var variant = variants[item.ProductVariantId];
            variant.StockQuantity -= item.Quantity;
            _db.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductVariantId = variant.Id,
                DeltaQuantity = -item.Quantity,
                Reason = "OrderPlacement"
            });
        }

        var finalAmount = Math.Max(0m, subtotal - discountAmount);
        var order = new Order
        {
            UserId = userId,
            TotalAmount = finalAmount,
            DiscountAmount = Math.Max(0m, discountAmount),
            PaymentMethod = paymentMethod,
            Status = paymentMethod == "COD" ? "Pending" : "PendingPayment",
            CouponId = couponId,
            CouponCode = couponCode
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        foreach (var item in cart.Items)
        {
            var variant = variants[item.ProductVariantId];
            _db.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductVariantId = variant.Id,
                ProductName = variant.Product?.Name ?? "Sản phẩm",
                VariantLabel = $"{variant.Ram}/{variant.Rom}/{variant.Color}",
                UnitPrice = variant.Price,
                Quantity = item.Quantity
            });
        }

        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return (order, null);
    }
}

