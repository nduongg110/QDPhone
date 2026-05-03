using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Data;
using QDPhone.Web.Services;
using System.Globalization;
using System.Security.Claims;

namespace QDPhone.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "StaffOrAdmin")]
[Route("admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IExportService _exportService;
    private readonly INotificationService _notificationService;
    private readonly IOrderService _orderService;

    public AdminController(ApplicationDbContext db, IExportService exportService, INotificationService notificationService, IOrderService orderService)
    {
        _db = db;
        _exportService = exportService;
        _notificationService = notificationService;
        _orderService = orderService;
    }

    [HttpGet("")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var from30Days = today.AddDays(-29);
        var from7Days = today.AddDays(-6);

        var orders = await _db.Orders.AsNoTracking().ToListAsync();
        var todayOrders = orders.Where(x => x.CreatedAt >= today && x.CreatedAt < today.AddDays(1)).ToList();
        var yesterdayOrders = orders.Where(x => x.CreatedAt >= yesterday && x.CreatedAt < today).ToList();
        var todayRevenue = todayOrders.Where(x => x.Status != "Cancelled").Sum(x => x.TotalAmount);
        var yesterdayRevenue = yesterdayOrders.Where(x => x.Status != "Cancelled").Sum(x => x.TotalAmount);
        var revenueChangePercent = yesterdayRevenue > 0
            ? Math.Round((todayRevenue - yesterdayRevenue) / yesterdayRevenue * 100m, 1)
            : (todayRevenue > 0 ? 100m : 0m);

        var statusBreakdown = orders
            .GroupBy(x => x.Status)
            .Select(g => new QDPhone.Web.Models.ViewModels.DashboardStatusPointViewModel
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToList();

        var recentOrders = orders
            .OrderByDescending(x => x.CreatedAt)
            .Take(8)
            .Select(x => new QDPhone.Web.Models.ViewModels.DashboardRecentOrderViewModel
            {
                Id = x.Id,
                CustomerName = x.UserId,
                TotalAmount = x.TotalAmount,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToList();

        var revenuePoints = new List<QDPhone.Web.Models.ViewModels.DashboardRevenuePointViewModel>();
        for (var d = from30Days; d <= today; d = d.AddDays(1))
        {
            var end = d.AddDays(1);
            var dayOrders = orders.Where(x => x.CreatedAt >= d && x.CreatedAt < end && x.Status != "Cancelled").ToList();
            revenuePoints.Add(new QDPhone.Web.Models.ViewModels.DashboardRevenuePointViewModel
            {
                Label = d.ToString("dd/MM"),
                Revenue = dayOrders.Sum(x => x.TotalAmount),
                Orders = dayOrders.Count
            });
        }

        var topProducts = await _db.OrderItems
            .Where(x => x.Order != null && x.Order.CreatedAt >= from7Days && x.Order.Status != "Cancelled")
            .GroupBy(x => x.ProductName)
            .Select(g => new
            {
                ProductName = g.Key,
                SoldQuantity = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.UnitPrice * x.Quantity)
            })
            .OrderByDescending(x => x.SoldQuantity)
            .Take(5)
            .ToListAsync();

        var productImages = await _db.Products
            .Include(x => x.Images)
            .Select(x => new
            {
                x.Name,
                Image = x.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault()
            })
            .ToListAsync();
        var imageMap = productImages.ToDictionary(x => x.Name, x => x.Image ?? "https://picsum.photos/seed/top-product/80/80");

        var activities = new List<QDPhone.Web.Models.ViewModels.DashboardActivityViewModel>();
        activities.AddRange(todayOrders.OrderByDescending(x => x.CreatedAt).Take(4).Select(x =>
            new QDPhone.Web.Models.ViewModels.DashboardActivityViewModel
            {
                Type = "order",
                Content = $"Đơn hàng #{x.Id} mới được tạo.",
                At = x.CreatedAt
            }));
        var lowStocks = await _db.ProductVariants
            .Include(x => x.Product)
            .Where(x => x.StockQuantity > 0 && x.StockQuantity <= 5)
            .OrderBy(x => x.StockQuantity)
            .Take(2)
            .ToListAsync();
        activities.AddRange(lowStocks.Select(x => new QDPhone.Web.Models.ViewModels.DashboardActivityViewModel
        {
            Type = "stock",
            Content = $"Sản phẩm {x.Product?.Name ?? "N/A"} sắp hết hàng (còn {x.StockQuantity}).",
            At = DateTime.UtcNow
        }));
        var recentReviews = await _db.Reviews.OrderByDescending(x => x.Id).Take(2).ToListAsync();
        activities.AddRange(recentReviews.Select(x => new QDPhone.Web.Models.ViewModels.DashboardActivityViewModel
        {
            Type = "review",
            Content = $"Đánh giá mới #{x.Id} được gửi.",
            At = DateTime.UtcNow
        }));

        var model = new QDPhone.Web.Models.ViewModels.AdminDashboardViewModel
        {
            TodayRevenue = todayRevenue,
            RevenueChangePercent = revenueChangePercent,
            NewOrdersCount = todayOrders.Count,
            PendingOrdersCount = orders.Count(x => x.Status is "Pending" or "PendingPayment"),
            NewUsersToday = 0,
            TotalUsers = await _db.Users.CountAsync(),
            LowStockCount = await _db.ProductVariants.CountAsync(x => x.StockQuantity > 0 && x.StockQuantity <= 5),
            TotalOrders = orders.Count,
            TotalProducts = await _db.Products.CountAsync(),
            TotalCategories = await _db.Categories.CountAsync(),
            TotalBrands = await _db.Brands.CountAsync(),
            PendingReviews = await _db.Reviews.CountAsync(),
            ActiveCoupons = await _db.Coupons.CountAsync(x => x.IsActive && x.EndAt >= DateTime.UtcNow),
            ActiveBanners = await _db.Banners.CountAsync(x => x.IsActive),
            RevenueLast30Days = revenuePoints,
            OrderStatusBreakdown = statusBreakdown,
            RecentOrders = recentOrders,
            TopProducts = topProducts.Select(x => new QDPhone.Web.Models.ViewModels.DashboardTopProductViewModel
            {
                ProductName = x.ProductName,
                SoldQuantity = x.SoldQuantity,
                Revenue = x.Revenue,
                ImageUrl = imageMap.TryGetValue(x.ProductName, out var img) ? img : "https://picsum.photos/seed/top-product/80/80"
            }).ToList(),
            Activities = activities.OrderByDescending(x => x.At).Take(8).ToList()
        };

        ViewBag.PendingOrdersCount = model.PendingOrdersCount;
        ViewBag.Breadcrumb = "Dashboard";
        return View(model);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("export-orders")]
    public async Task<FileResult> ExportOrders()
    {
        var data = await _db.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var userIds = data.Select(x => x.UserId).Distinct().ToList();

        var userMap = await _db.Users
            .Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.FullName ?? x.UserName ?? x.Email ?? x.Id);

        var rows = data.Select(x => new QDPhone.Web.Models.ViewModels.OrderExportRowViewModel
        {
            OrderId = x.Id,
            UserName = userMap.TryGetValue(x.UserId, out var name) ? name : x.UserId,

            ProductName = x.Items.Any()
                ? string.Join(", ",
                    x.Items
                     .Where(i => !string.IsNullOrEmpty(i.ProductName))
                     .GroupBy(i => i.ProductName)
                     .Select(g => $"{g.Key} ({g.Sum(i => i.Quantity)})"))
                : "N/A",

            Status = x.Status,
            TotalAmount = x.TotalAmount
        });

        var bytes = _exportService.ExportOrdersToExcel(rows);

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "orders.xlsx"
        );
    }

    [Authorize(Policy = "StaffOrAdmin")]
    [HttpGet("orders")]
    public async Task<IActionResult> Orders(string? status, string? paymentMethod, DateTime? fromDate, DateTime? toDate, string? q, int page = 1, int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 10, 100);

        var baseQuery = _db.Orders.AsNoTracking();
        var statusCounts = new QDPhone.Web.Models.ViewModels.AdminOrderStatusCountViewModel
        {
            All = await baseQuery.CountAsync(),
            Pending = await baseQuery.CountAsync(x => x.Status == "Pending" || x.Status == "PendingPayment"),
            Processing = await baseQuery.CountAsync(x => x.Status == "Paid"),
            Shipping = await baseQuery.CountAsync(x => x.Status == "Shipping"),
            Completed = await baseQuery.CountAsync(x => x.Status == "Done"),
            Cancelled = await baseQuery.CountAsync(x => x.Status == "Cancelled" || x.Status == "PaymentFailed")
        };

        IQueryable<QDPhone.Web.Models.Entities.Order> filtered = baseQuery;
        filtered = status switch
        {
            "pending" => filtered.Where(x => x.Status == "Pending" || x.Status == "PendingPayment"),
            "processing" => filtered.Where(x => x.Status == "Paid"),
            "shipping" => filtered.Where(x => x.Status == "Shipping"),
            "completed" => filtered.Where(x => x.Status == "Done"),
            "cancelled" => filtered.Where(x => x.Status == "Cancelled" || x.Status == "PaymentFailed"),
            _ => filtered
        };

        if (!string.IsNullOrWhiteSpace(paymentMethod))
            filtered = filtered.Where(x => x.PaymentMethod == paymentMethod);
        if (fromDate.HasValue)
            filtered = filtered.Where(x => x.CreatedAt >= fromDate.Value.Date);
        if (toDate.HasValue)
            filtered = filtered.Where(x => x.CreatedAt <= toDate.Value.Date.AddDays(1).AddTicks(-1));
        if (!string.IsNullOrWhiteSpace(q))
        {
            var keyword = q.Trim();
            filtered = filtered.Where(x => x.UserId.Contains(keyword) || EF.Functions.Like(x.Id.ToString(), $"%{keyword}%"));
        }

        var totalItems = await filtered.CountAsync();
        var pageOrders = await filtered
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.TotalAmount,
                x.DiscountAmount,
                x.CouponCode,
                x.PaymentMethod,
                x.Status,
                x.CreatedAt,
                FirstProductName = x.Items.OrderBy(i => i.Id).Select(i => i.ProductName).FirstOrDefault(),
                MoreProductCount = Math.Max(0, x.Items.Count - 1)
            })
            .ToListAsync();

        var userIds = pageOrders.Select(x => x.UserId).Distinct().ToList();
        var users = await _db.Users.Where(x => userIds.Contains(x.Id))
            .Select(x => new { x.Id, x.FullName, x.PhoneNumber })
            .ToListAsync();
        var userMap = users.ToDictionary(x => x.Id, x => x);
        var rows = pageOrders
            .Select(x => new QDPhone.Web.Models.ViewModels.AdminOrderRowViewModel
            {
                Id = x.Id,
                UserId = x.UserId,
                CustomerName = x.UserId,
                PhoneNumber = "-",
                FirstProductName = x.FirstProductName ?? "N/A",
                MoreProductCount = x.MoreProductCount,
                TotalAmount = x.TotalAmount,
                DiscountAmount = x.DiscountAmount,
                CouponCode = x.CouponCode,
                PaymentMethod = x.PaymentMethod,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToList();
        foreach (var row in rows)
        {
            if (userMap.TryGetValue(row.UserId, out var user))
            {
                row.CustomerName = user.FullName ?? row.UserId;
                row.PhoneNumber = user.PhoneNumber ?? "-";
            }
        }

        ViewBag.Breadcrumb = "Đơn hàng";
        ViewBag.PendingOrdersCount = statusCounts.Pending;
        return View(new QDPhone.Web.Models.ViewModels.AdminOrderIndexViewModel
        {
            Rows = rows,
            Status = status,
            PaymentMethod = paymentMethod,
            FromDate = fromDate,
            ToDate = toDate,
            Q = q,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            StatusCounts = statusCounts
        });
    }

    [Authorize(Policy = "StaffOrAdmin")]
    [HttpPost]
    [Route("orders/update-status")]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
    {
        var allowed = new[] { "Pending", "PendingPayment", "Paid", "Shipping", "Done", "Cancelled", "PaymentFailed" };
        if (!allowed.Contains(status)) return BadRequest();
        var order = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orderId);
        if (order == null) return NotFound();
        var changed = await _orderService.UpdatePaymentStatusAsync(orderId, status);
        if (!changed)
        {
            TempData["Message"] = $"Không thể chuyển trạng thái từ {order.Status} sang {status}.";
            return RedirectToAction(nameof(Orders));
        }

        if (status == "PaymentFailed")
            await _orderService.RestoreStockForFailedPaymentAsync(orderId);
        var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        _db.AdminAuditLogs.Add(new QDPhone.Web.Models.Entities.AdminAuditLog
        {
            ActorUserId = actorUserId,
            Action = "UpdateOrderStatus",
            TargetType = "Order",
            TargetId = orderId.ToString(),
            Detail = $"From={order.Status};To={status}"
        });
        await _db.SaveChangesAsync();
        await _notificationService.NotifyOrderStatusChangedAsync(order.UserId, order.Id, status);
        return RedirectToAction(nameof(Orders));
    }
}

