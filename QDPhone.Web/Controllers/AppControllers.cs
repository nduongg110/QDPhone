using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QDPhone.Web.Controllers;

public class ProductsController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Details(int id) => View();
}

[Authorize(Policy = "StaffOrAdmin")]
[Route("admin")]
public class AdminController : Controller
{
    [HttpGet("")]
    [HttpGet("dashboard")]
    public IActionResult Dashboard()
    {
        ViewBag.Breadcrumb = "Dashboard";
        return View();
    }

    [HttpGet("orders")]
    public IActionResult Orders()
    {
        ViewBag.Breadcrumb = "Đơn hàng";
        return View();
    }
}

[Authorize(Policy = "StaffOrAdmin")]
[Route("admin/coupons")]
public class CouponsController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewBag.Breadcrumb = "Mã giảm giá";
        return View();
    }
}
