using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QDPhone.Web.Services;
using System.Security.Claims;

namespace QDPhone.Web.Controllers;

[Authorize]
public class WishlistController : Controller
{
    private readonly IWishlistService _wishlistService;
    public WishlistController(IWishlistService wishlistService) => _wishlistService = wishlistService;

    [HttpPost]
    public async Task<IActionResult> Add(int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _wishlistService.AddAsync(userId, productId);
        return RedirectToAction("Index", "Products");
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _wishlistService.RemoveAsync(userId, productId);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return View(await _wishlistService.GetItemsAsync(userId));
    }
}

