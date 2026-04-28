using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QDPhone.Web.Controllers;

[Authorize(Policy = "StaffOrAdmin")]
[Route("admin/brands")]
public class BrandsController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewBag.Breadcrumb = "Thương hiệu";
        return View();
    }
}

[Authorize(Policy = "StaffOrAdmin")]
[Route("admin/categories")]
public class CategoriesController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewBag.Breadcrumb = "Danh mục";
        return View();
    }
}

[Authorize(Policy = "StaffOrAdmin")]
[Route("admin/products")]
public class AdminProductsController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewBag.Breadcrumb = "Sản phẩm";
        return View();
    }
}
