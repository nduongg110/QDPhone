using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Data;
using QDPhone.Web.Services;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace QDPhone.Web.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly ICartService _cartService;
    private readonly ICouponService _couponService;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<CheckoutController> _logger;
    private readonly INotificationService _notificationService;

    public CheckoutController(
        IOrderService orderService,
        IPaymentService paymentService,
        ICouponService couponService,
        ICartService cartService,
        ApplicationDbContext db,
        ILogger<CheckoutController> logger,
        INotificationService notificationService)
    {
        _orderService = orderService;
        _paymentService = paymentService;
        _couponService = couponService;
        _cartService = cartService;
        _db = db;
        _logger = logger;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var cart = await _cartService.GetCartViewAsync(userId);
        var appliedCode = TempData["AppliedCouponCode"] as string;
        if (!string.IsNullOrWhiteSpace(appliedCode))
        {
            var (coupon, discount, reason) = await _couponService.TryApplyCouponAsync(appliedCode, cart.SubTotal, userId);
            if (coupon != null)
            {
                ViewBag.AppliedCouponCode = coupon.Code;
                ViewBag.AppliedCouponDiscount = discount;
                ViewBag.AppliedFinalTotal = Math.Max(0m, cart.SubTotal - discount);
                ViewBag.AppliedCouponMessage = $"Đã áp dụng mã {coupon.Code}.";
            }
            else
            {
                ViewBag.AppliedCouponMessage = reason ?? "Mã khuyến mãi không hợp lệ.";
            }
        }

        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> ApplyCoupon(string? couponCode)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var cart = await _cartService.GetCartViewAsync(userId);
        if (string.IsNullOrWhiteSpace(couponCode))
        {
            TempData["Message"] = "Vui lòng nhập mã khuyến mãi.";
            return RedirectToAction(nameof(Index));
        }

        var (coupon, _, reason) = await _couponService.TryApplyCouponAsync(couponCode, cart.SubTotal, userId);
        if (coupon == null)
        {
            TempData["Message"] = reason ?? "Mã khuyến mãi không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        TempData["AppliedCouponCode"] = coupon.Code;
        TempData["Message"] = $"Áp dụng mã {coupon.Code} thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> PlaceCod(string? couponCode)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var cart = await _cartService.GetCartViewAsync(userId);
        var couponId = 0;
        decimal discount = 0m;
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var (coupon, computedDiscount, reason) = await _couponService.TryApplyCouponAsync(couponCode, cart.SubTotal, userId);
            if (coupon != null)
            {
                discount = computedDiscount;
                couponId = coupon.Id;
                TempData["Message"] = $"Áp dụng mã {coupon.Code} thành công.";
            }
            else
            {
                TempData["Message"] = reason ?? "Mã khuyến mãi không hợp lệ.";
            }
        }

        var (order, error) = await _orderService.PlaceOrderFromCartAsync(userId, discount, "COD", couponId > 0 ? couponId : null, couponId > 0 ? couponCode?.Trim().ToUpperInvariant() : null);
        if (order == null)
        {
            TempData["Message"] = error ?? "Thanh toán thất bại.";
            return RedirectToAction(nameof(Index));
        }

        if (couponId > 0) await _couponService.ConsumeAsync(couponId, userId, order.Id);
        await _notificationService.NotifyOrderCreatedAsync(userId, order.Id, order.TotalAmount);
        TempData["Message"] = $"Đơn hàng #{order.Id} đã được tạo.";
        return RedirectToAction("Success", "Orders", new { id = order.Id });
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayOs(string? couponCode)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var cart = await _cartService.GetCartViewAsync(userId);
        var finalAmount = cart.SubTotal;
        var couponId = 0;
        decimal discount = 0m;
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var (coupon, computedDiscount, reason) = await _couponService.TryApplyCouponAsync(couponCode, cart.SubTotal, userId);
            if (coupon != null)
            {
                discount = computedDiscount;
                couponId = coupon.Id;
                finalAmount -= discount;
            }
            else
            {
                TempData["Message"] = reason ?? "Mã khuyến mãi không hợp lệ.";
            }
        }

        var (order, error) = await _orderService.PlaceOrderFromCartAsync(userId, discount, "PAYOS", couponId > 0 ? couponId : null, couponId > 0 ? couponCode?.Trim().ToUpperInvariant() : null);
        if (order == null)
        {
            TempData["Message"] = error ?? "Thanh toán thất bại.";
            return RedirectToAction(nameof(Index));
        }

        await _notificationService.NotifyOrderCreatedAsync(userId, order.Id, order.TotalAmount);
        try
        {
            var paymentUrl = await _paymentService.CreatePayOsCheckoutUrlAsync(order.Id, finalAmount);
            return Redirect(paymentUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create PayOS payment link failed for order {OrderId}", order.Id);
            await _orderService.UpdatePaymentStatusAsync(order.Id, "PaymentFailed");
            await _orderService.RestoreStockForFailedPaymentAsync(order.Id);
            TempData["Message"] = "Không tạo được link thanh toán PayOS. Vui lòng kiểm tra cấu hình API và thử lại.";
            return RedirectToAction(nameof(Index));
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> PayOsCallback(string status, int orderCode, string amount, string signature)
    {
        var result = await ProcessPayOsPaymentResultAsync(orderCode, amount, status, signature);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage ?? "Invalid payload");
        var mappedStatus = result.MappedStatus ?? "PaymentFailed";
        var vnStatus = mappedStatus == "Paid" ? "Đã thanh toán" : "Thanh toán thất bại";
        TempData["Message"] = $"PayOS phản hồi cho đơn {orderCode} với trạng thái {vnStatus}.";
        if (mappedStatus == "Paid")
            return RedirectToAction(nameof(PayOsSuccess), new { orderCode, status = "PAID" });
        return RedirectToAction("MyOrders", "Orders");
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Cancel(string? code, string? id, bool? cancel, string? status, int? orderCode)
    {
        var vm = new PayOsCancelViewModel
        {
            OrderCode = orderCode,
            Code = code,
            TransactionId = id,
            IsCancelled = cancel ?? true,
            Status = string.IsNullOrWhiteSpace(status) ? "CANCELLED" : status
        };
        return View(vm);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> PayOsSuccess(int orderCode, string? status, string? id, string? code)
    {
        var order = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orderCode);
        var vm = new PayOsSuccessViewModel
        {
            OrderCode = orderCode,
            PaymentMethod = order?.PaymentMethod ?? "PAYOS",
            TotalAmount = order?.TotalAmount ?? 0,
            OrderStatus = order?.Status ?? "Paid",
            PayOsStatus = string.IsNullOrWhiteSpace(status) ? "PAID" : status,
            PayOsCode = code,
            TransactionId = id
        };
        return View(vm);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> PayOsWebhook([FromBody] PayOsWebhookPayload payload)
    {
        if (payload == null) return BadRequest(new { code = "01", desc = "Payload không hợp lệ" });
        _logger.LogInformation("PayOS webhook payload: {Payload}", JsonSerializer.Serialize(payload));
        var orderCode = payload.Data?.OrderCode ?? payload.OrderCode;
        var amount = payload.Data?.Amount?.ToString(CultureInfo.InvariantCulture) ?? payload.Amount?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        var status = payload.Data?.Status ?? payload.Status;
        var signature = payload.Data?.Signature ?? payload.Signature;
        var transactionRef = payload.Data?.Reference ?? payload.Data?.TransactionId;

        var result = await ProcessPayOsPaymentResultAsync(orderCode, amount, status, signature, transactionRef);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("PayOS webhook rejected for order {OrderCode}: {Error}", orderCode, result.ErrorMessage);
            return BadRequest(new { code = "01", desc = result.ErrorMessage ?? "Invalid payload" });
        }

        return Ok(new { code = "00", desc = "success" });
    }

    private async Task<PayOsProcessResult> ProcessPayOsPaymentResultAsync(
        int orderCode,
        string amount,
        string status,
        string signature,
        string? externalRef = null)
    {
        if (orderCode <= 0 || string.IsNullOrWhiteSpace(status) || string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(amount))
            return PayOsProcessResult.Fail("Thiếu dữ liệu callback.");

        var normalizedAmount = amount.Replace(',', '.').Trim();
        if (!decimal.TryParse(normalizedAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out var callbackAmount))
            return PayOsProcessResult.Fail("Amount không hợp lệ.");

        if (!_paymentService.VerifyPayOsSignature(orderCode, normalizedAmount, status, signature))
            return PayOsProcessResult.Fail("Sai chữ ký.");

        var signaturePrefix = signature[..Math.Min(8, signature.Length)];
        var externalTransactionId = !string.IsNullOrWhiteSpace(externalRef)
            ? $"payos-{externalRef}"
            : $"payos-{orderCode}-{status}-{signaturePrefix}";
        var existed = await _db.PaymentTransactions.AnyAsync(x => x.ExternalTransactionId == externalTransactionId);
        if (existed)
        {
            _logger.LogInformation("Duplicate PayOS callback/webhook ignored for order {OrderCode}", orderCode);
            return PayOsProcessResult.Success(MapPayOsStatus(status));
        }

        var order = await _db.Orders.FindAsync(orderCode);
        if (order == null)
            return PayOsProcessResult.Fail("Không tìm thấy đơn hàng.");

        if (Math.Abs(order.TotalAmount - callbackAmount) > 1m)
            return PayOsProcessResult.Fail("Amount callback không khớp đơn hàng.");

        var mappedStatus = MapPayOsStatus(status);
        await _orderService.UpdatePaymentStatusAsync(orderCode, mappedStatus);
        if (order is { CouponId: > 0 } && mappedStatus == "Paid")
            await _couponService.ConsumeAsync(order.CouponId.Value, order.UserId, order.Id);
        if (mappedStatus == "PaymentFailed")
            await _orderService.RestoreStockForFailedPaymentAsync(orderCode);

        if (order != null)
            await _notificationService.NotifyOrderStatusChangedAsync(order.UserId, order.Id, mappedStatus);

        _db.PaymentTransactions.Add(new Models.Entities.PaymentTransaction
        {
            OrderId = orderCode,
            ExternalTransactionId = externalTransactionId,
            Status = mappedStatus,
            Gateway = "PayOS"
        });
        await _db.SaveChangesAsync();
        return PayOsProcessResult.Success(mappedStatus);
    }

    private static string MapPayOsStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status)) return "PaymentFailed";
        return status.Trim().ToUpperInvariant() switch
        {
            "SUCCESS" or "PAID" => "Paid",
            "PENDING" or "PROCESSING" => "PendingPayment",
            "CANCELLED" or "CANCELED" or "FAILED" or "FAIL" or "EXPIRED" => "PaymentFailed",
            _ => "PaymentFailed"
        };
    }

    private sealed class PayOsProcessResult
    {
        public bool IsSuccess { get; init; }
        public string? MappedStatus { get; init; }
        public string? ErrorMessage { get; init; }

        public static PayOsProcessResult Success(string mappedStatus) => new() { IsSuccess = true, MappedStatus = mappedStatus };
        public static PayOsProcessResult Fail(string message) => new() { IsSuccess = false, ErrorMessage = message };
    }

    public sealed class PayOsWebhookPayload
    {
        public int OrderCode { get; set; }
        public decimal? Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public PayOsWebhookData? Data { get; set; }
    }

    public sealed class PayOsWebhookData
    {
        public int OrderCode { get; set; }
        public decimal? Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public string? TransactionId { get; set; }
    }

    public sealed class PayOsCancelViewModel
    {
        public int? OrderCode { get; set; }
        public string? Code { get; set; }
        public string? TransactionId { get; set; }
        public bool IsCancelled { get; set; }
        public string Status { get; set; } = "CANCELLED";
    }

    public sealed class PayOsSuccessViewModel
    {
        public int OrderCode { get; set; }
        public string PaymentMethod { get; set; } = "PAYOS";
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = "Paid";
        public string PayOsStatus { get; set; } = "PAID";
        public string? PayOsCode { get; set; }
        public string? TransactionId { get; set; }
    }
}

