using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Data;

namespace QDPhone.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "StaffOrAdmin")]
[Route("admin/coupons")]
public class CouponsController : Controller
{
    private readonly ApplicationDbContext _db;
    public CouponsController(ApplicationDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index(string? status, string? keyword, DateTime? expireFrom, DateTime? expireTo, string? sort, int page = 1, int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 10, 100);

        var coupons = await _db.Coupons.OrderByDescending(x => x.Id).ToListAsync();
        var usageMap = await _db.CouponUsages
            .GroupBy(x => x.CouponId)
            .Select(g => new { CouponId = g.Key, UsedCount = g.Count() })
            .ToDictionaryAsync(x => x.CouponId, x => x.UsedCount);
        var rows = coupons.Select(c => new QDPhone.Web.Models.ViewModels.CouponAdminRowViewModel
        {
            Coupon = c,
            UsedCount = usageMap.TryGetValue(c.Id, out var count) ? count : 0
        });

        rows = status switch
        {
            "active" => rows.Where(x => x.Coupon.IsActive && !x.IsExpired && !x.IsExhausted),
            "expired" => rows.Where(x => x.IsExpired),
            "exhausted" => rows.Where(x => x.IsExhausted),
            _ => rows
        };

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToUpperInvariant();
            rows = rows.Where(x => x.Coupon.Code.Contains(kw));
        }
        if (expireFrom.HasValue)
            rows = rows.Where(x => x.Coupon.EndAt >= expireFrom.Value.Date);
        if (expireTo.HasValue)
            rows = rows.Where(x => x.Coupon.EndAt <= expireTo.Value.Date.AddDays(1).AddTicks(-1));

        rows = sort switch
        {
            "code_asc" => rows.OrderBy(x => x.Coupon.Code),
            "used_desc" => rows.OrderByDescending(x => x.UsedCount),
            "expire_asc" => rows.OrderBy(x => x.Coupon.EndAt),
            _ => rows.OrderByDescending(x => x.Coupon.Id)
        };

        var totalItems = rows.Count();
        var pageRows = rows
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return View(new QDPhone.Web.Models.ViewModels.CouponAdminIndexViewModel
        {
            Rows = pageRows,
            Status = status,
            Keyword = keyword,
            ExpireFrom = expireFrom,
            ExpireTo = expireTo,
            Sort = sort,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        });
    }

    [HttpGet("create")]
    public IActionResult Create() => View();

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> Create(QDPhone.Web.Models.Entities.Coupon coupon)
    {
        ValidateCouponModel(coupon);
        if (!ModelState.IsValid) return View(coupon);
        coupon.Code = coupon.Code.Trim().ToUpperInvariant();
        try
        {
            _db.Coupons.Add(coupon);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(coupon.Code), "Mã coupon đã tồn tại.");
            return View(coupon);
        }
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var coupon = await _db.Coupons.FindAsync(id);
        if (coupon == null) return NotFound();
        return View(coupon);
    }

    [HttpPost]
    [Route("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, QDPhone.Web.Models.Entities.Coupon model)
    {
        if (id != model.Id) return BadRequest();
        ValidateCouponModel(model);
        if (!ModelState.IsValid) return View(model);
        var coupon = await _db.Coupons.FindAsync(id);
        if (coupon == null) return NotFound();

        coupon.Code = model.Code.Trim().ToUpperInvariant();
        coupon.IsPercentage = model.IsPercentage;
        coupon.Value = model.Value;
        coupon.MaxDiscount = model.MaxDiscount;
        coupon.MinOrderAmount = model.MinOrderAmount;
        coupon.UsageLimit = model.UsageLimit;
        coupon.UsagePerUserLimit = model.UsagePerUserLimit;
        coupon.StartAt = model.StartAt;
        coupon.EndAt = model.EndAt;
        coupon.IsActive = model.IsActive;
        try
        {
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(model.Code), "Mã coupon đã tồn tại.");
            return View(model);
        }
    }

    [HttpPost]
    [Route("delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var coupon = await _db.Coupons.FindAsync(id);
        if (coupon == null) return RedirectToAction(nameof(Index));

        var usedCount = await _db.CouponUsages.CountAsync(x => x.CouponId == id);
        if (usedCount > 0)
        {
            coupon.IsActive = false;
            TempData["Message"] = $"Mã {coupon.Code} đã phát sinh {usedCount} lượt dùng, hệ thống đã chuyển sang trạng thái tắt thay vì xóa.";
        }
        else
        {
            _db.Coupons.Remove(coupon);
            TempData["Message"] = $"Đã xóa mã {coupon.Code}.";
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void ValidateCouponModel(QDPhone.Web.Models.Entities.Coupon coupon)
    {
        if (string.IsNullOrWhiteSpace(coupon.Code))
            ModelState.AddModelError(nameof(coupon.Code), "Mã coupon không được để trống.");
        if (coupon.Value <= 0)
            ModelState.AddModelError(nameof(coupon.Value), "Giá trị giảm phải lớn hơn 0.");
        if (coupon.IsPercentage && coupon.Value > 100)
            ModelState.AddModelError(nameof(coupon.Value), "Coupon theo % không được lớn hơn 100.");
        if (coupon.StartAt >= coupon.EndAt)
            ModelState.AddModelError(nameof(coupon.EndAt), "Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");
        if (coupon.UsageLimit < 0)
            ModelState.AddModelError(nameof(coupon.UsageLimit), "Giới hạn lượt dùng không được âm.");
        if (coupon.UsagePerUserLimit < 0)
            ModelState.AddModelError(nameof(coupon.UsagePerUserLimit), "Giới hạn theo người dùng không được âm.");
        if (coupon.MaxDiscount.HasValue && coupon.MaxDiscount.Value <= 0)
            ModelState.AddModelError(nameof(coupon.MaxDiscount), "Giảm tối đa phải lớn hơn 0.");
        if (coupon.MinOrderAmount.HasValue && coupon.MinOrderAmount.Value < 0)
            ModelState.AddModelError(nameof(coupon.MinOrderAmount), "Đơn tối thiểu không được âm.");
    }
}

