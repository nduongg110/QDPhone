using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QDPhone.Web.Services;
using System.Security.Claims;

namespace QDPhone.Web.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ICartService _cartService;
    public CartController(ICartService cartService) => _cartService = cartService;

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return View(await _cartService.GetCartViewAsync(userId));
    }

    [HttpPost]
    public async Task<IActionResult> Update(int cartItemId, int quantity)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await _cartService.UpdateItemAsync(userId, cartItemId, quantity);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Message"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int cartItemId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _cartService.RemoveItemAsync(userId, cartItemId);
        return RedirectToAction(nameof(Index));
    }
}

