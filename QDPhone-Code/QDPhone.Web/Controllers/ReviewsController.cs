using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Data;
using System.Security.Claims;

namespace QDPhone.Web.Controllers;

[Authorize]
public class ReviewsController : Controller
{
    private readonly ApplicationDbContext _db;
    public ReviewsController(ApplicationDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Create(int productId, int rating, string comment)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var hasPurchased = await _db.OrderItems
            .AsNoTracking()
            .Where(x => x.Order != null &&
                        x.Order.UserId == userId &&
                        x.Order.Status != "Cancelled" &&
                        x.Order.Status != "PaymentFailed")
            .AnyAsync(x => x.ProductVariant != null && x.ProductVariant.ProductId == productId);
        if (!hasPurchased)
        {
            TempData["Message"] = "Bạn cần mua sản phẩm này trước khi đánh giá.";
            return RedirectToAction("Details", "Products", new { id = productId });
        }

        rating = Math.Clamp(rating, 1, 5);
        comment = (comment ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(comment))
        {
            TempData["Message"] = "Vui lòng nhập nội dung đánh giá.";
            return RedirectToAction("Details", "Products", new { id = productId });
        }

        _db.Reviews.Add(new Models.Entities.Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = rating,
            Comment = comment,
            IsApproved = true
        });
        await _db.SaveChangesAsync();
        TempData["Message"] = "Đánh giá của bạn đã được ghi nhận.";
        return RedirectToAction("Details", "Products", new { id = productId });
    }
}

