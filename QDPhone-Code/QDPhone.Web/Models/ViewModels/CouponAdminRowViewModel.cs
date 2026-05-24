using QDPhone.Web.Models.Entities;

namespace QDPhone.Web.Models.ViewModels;

public class CouponAdminRowViewModel
{
    public Coupon Coupon { get; set; } = new();
    public int UsedCount { get; set; }
    public int RemainingCount => Coupon.UsageLimit <= 0 ? int.MaxValue : Math.Max(0, Coupon.UsageLimit - UsedCount);
    public bool IsExpired => Coupon.EndAt < DateTime.Now;
    public bool IsExhausted => Coupon.UsageLimit > 0 && UsedCount >= Coupon.UsageLimit;
}
