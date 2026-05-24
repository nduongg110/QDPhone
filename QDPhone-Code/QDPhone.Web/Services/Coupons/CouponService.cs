using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Data;
using QDPhone.Web.Models.Entities;

namespace QDPhone.Web.Services;

public interface ICouponService
{
    decimal CalculateDiscount(Coupon coupon, decimal subtotal);
    Task<Coupon?> FindValidCouponAsync(string code, decimal subtotal, string userId);
    Task<(Coupon? coupon, decimal discount, string? reason)> TryApplyCouponAsync(string code, decimal subtotal, string userId);
    Task ConsumeAsync(int couponId, string userId, int orderId);
}

public class CouponService : ICouponService
{
    private readonly ApplicationDbContext _db;
    public CouponService(ApplicationDbContext db) => _db = db;

    public decimal CalculateDiscount(Coupon coupon, decimal subtotal)
    {
        var now = DateTime.Now;
        if (!coupon.IsActive || now < coupon.StartAt || now > coupon.EndAt)
            return 0m;
        if (coupon.MinOrderAmount.HasValue && subtotal < coupon.MinOrderAmount.Value)
            return 0m;
        var discount = coupon.IsPercentage ? subtotal * coupon.Value / 100m : coupon.Value;
        if (coupon.MaxDiscount.HasValue) discount = Math.Min(discount, coupon.MaxDiscount.Value);
        return Math.Max(0m, discount);
    }

    public async Task<Coupon?> FindValidCouponAsync(string code, decimal subtotal, string userId)
    {
        var result = await TryApplyCouponAsync(code, subtotal, userId);
        return result.coupon;
    }

    public async Task<(Coupon? coupon, decimal discount, string? reason)> TryApplyCouponAsync(string code, decimal subtotal, string userId)
    {
        if (string.IsNullOrWhiteSpace(code))
            return (null, 0m, "Vui lòng nhập mã khuyến mãi.");

        var normalized = code.Trim().ToUpperInvariant();
        var coupon = await _db.Coupons.FirstOrDefaultAsync(x => x.Code == normalized && x.IsActive);
        if (coupon == null) return (null, 0m, "Mã khuyến mãi không tồn tại hoặc đã tắt.");
        var totalUsage = await _db.CouponUsages.CountAsync(x => x.CouponId == coupon.Id);
        if (coupon.UsageLimit > 0 && totalUsage >= coupon.UsageLimit)
            return (null, 0m, "Mã đã hết lượt sử dụng.");
        var userUsage = await _db.CouponUsages.CountAsync(x => x.CouponId == coupon.Id && x.UserId == userId);
        if (coupon.UsagePerUserLimit > 0 && userUsage >= coupon.UsagePerUserLimit)
            return (null, 0m, "Bạn đã dùng hết lượt cho mã này.");
        if (DateTime.Now < coupon.StartAt)
            return (null, 0m, $"Mã sẽ có hiệu lực từ {coupon.StartAt:dd/MM/yyyy HH:mm}.");
        if (DateTime.Now > coupon.EndAt)
            return (null, 0m, "Mã đã hết hạn.");
        if (coupon.MinOrderAmount.HasValue && subtotal < coupon.MinOrderAmount.Value)
            return (null, 0m, $"Đơn tối thiểu để dùng mã là {coupon.MinOrderAmount.Value:N0}đ.");
        var discount = CalculateDiscount(coupon, subtotal);
        if (discount <= 0) return (null, 0m, "Mã khuyến mãi không áp dụng cho đơn hàng này.");
        return (coupon, discount, null);
    }

    public async Task ConsumeAsync(int couponId, string userId, int orderId)
    {
        var exists = await _db.CouponUsages.AnyAsync(x => x.CouponId == couponId && x.UserId == userId && x.OrderId == orderId);
        if (exists) return;
        _db.CouponUsages.Add(new CouponUsage { CouponId = couponId, UserId = userId, OrderId = orderId });
        await _db.SaveChangesAsync();
    }
}

