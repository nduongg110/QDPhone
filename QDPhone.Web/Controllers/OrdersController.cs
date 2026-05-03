using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Data;
using QDPhone.Web.Services;
using System.Security.Claims;

namespace QDPhone.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ApplicationDbContext db, IPaymentService paymentService, ILogger<OrdersController> logger)
    {
        _db = db;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<IActionResult> MyOrders()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var orders = await _db.Orders.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).ToListAsync();
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var order = await _db.Orders
            .Include(x => x.Items)
            .ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (order == null) return NotFound();
        return View(order);
    }

    [HttpGet]
    public async Task<IActionResult> Success(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var order = await _db.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (order == null) return NotFound();
        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> RetryPayment(int orderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var order = await _db.Orders.FirstOrDefaultAsync(x => x.Id == orderId && x.UserId == userId);
        if (order == null) return NotFound();
        if (order.PaymentMethod != "PAYOS") return BadRequest("Đơn hàng không dùng PayOS.");
        if (order.Status is "Paid" or "Done") return BadRequest("Đơn hàng đã thanh toán.");
        order.Status = "PendingPayment";
        await _db.SaveChangesAsync();
        try
        {
            var paymentUrl = await _paymentService.CreatePayOsCheckoutUrlAsync(order.Id, order.TotalAmount);
            return Redirect(paymentUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Retry PayOS payment link failed for order {OrderId}", orderId);
            TempData["Message"] = "Không thể tạo lại link PayOS lúc này.";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
}

