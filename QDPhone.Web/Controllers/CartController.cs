using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QDPhone.Web.Services;
using System.Security.Claims;

namespace QDPhone.Web.Controllers;

// Bắt buộc phải đăng nhập mới truy cập được toàn bộ controller này
[Authorize]
public class CartController : Controller
{
    // Service xử lý logic giỏ hàng (lấy dữ liệu, update, xóa...)
    private readonly ICartService _cartService;

    // Inject service thông qua constructor
    public CartController(ICartService cartService) => _cartService = cartService;

    // ================= HIỂN THỊ GIỎ HÀNG =================
    public async Task<IActionResult> Index()
    {
        // Lấy userId từ token (Claim)
        // NameIdentifier chính là Id của user trong Identity
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Gọi service để lấy dữ liệu giỏ hàng của user
        var cart = await _cartService.GetCartViewAsync(userId);

        // Trả dữ liệu sang View
        return View(cart);
    }

    // ================= CẬP NHẬT SỐ LƯỢNG =================
    [HttpPost]
    public async Task<IActionResult> Update(int cartItemId, int quantity)
    {
        // Lấy userId hiện tại
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        try
        {
            // Gọi service cập nhật số lượng sản phẩm trong giỏ
            await _cartService.UpdateItemAsync(userId, cartItemId, quantity);
        }
        catch (InvalidOperationException ex)
        {
            // Nếu có lỗi (vd: số lượng vượt quá tồn kho...)
            // Lưu message vào TempData để hiển thị ra View
            TempData["Message"] = ex.Message;
        }

        // Sau khi xử lý xong -> reload lại trang giỏ hàng
        return RedirectToAction(nameof(Index));
    }

    // ================= XÓA SẢN PHẨM KHỎI GIỎ =================
    [HttpPost]
    public async Task<IActionResult> Remove(int cartItemId)
    {
        // Lấy userId
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Gọi service xóa item khỏi giỏ
        await _cartService.RemoveItemAsync(userId, cartItemId);

        // Reload lại trang
        return RedirectToAction(nameof(Index));
    }
}