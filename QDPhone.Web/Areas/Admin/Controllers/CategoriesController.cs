using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QDPhone.Web.Data;
using QDPhone.Web.Models.Entities;

namespace QDPhone.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "StaffOrAdmin")]
[Route("admin/categories")]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IMemoryCache _cache;

    public CategoriesController(ApplicationDbContext db, IWebHostEnvironment env, IMemoryCache cache)
    {
        _db = db;
        _env = env;
        _cache = cache;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await _db.Categories.OrderBy(x => x.Name).ToListAsync());

    [HttpGet("create")]
    public IActionResult Create() => View();

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> Create(Category model, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(model);
        if (imageFile != null && imageFile.Length > 0)
            model.ImageUrl = await SaveUploadAsync(imageFile, "categories");
        _db.Categories.Add(model);
        await _db.SaveChangesAsync();
        _cache.Remove("home-page-vm");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, Category model, IFormFile? imageFile)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();
        category.Name = model.Name;
        category.Slug = model.Slug;
        if (imageFile != null && imageFile.Length > 0)
            category.ImageUrl = await SaveUploadAsync(imageFile, "categories");
        else
            category.ImageUrl = model.ImageUrl;
        await _db.SaveChangesAsync();
        _cache.Remove("home-page-vm");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return RedirectToAction(nameof(Index));
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        _cache.Remove("home-page-vm");
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> SaveUploadAsync(IFormFile file, string folderName)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        if (!allowed.Contains(ext)) throw new InvalidOperationException("Định dạng ảnh không hợp lệ.");
        var uploadRoot = Path.Combine(_env.WebRootPath, "uploads", folderName);
        Directory.CreateDirectory(uploadRoot);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadRoot, fileName);
        await using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream);
        return $"/uploads/{folderName}/{fileName}";
    }
}

