using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Data;
using QDPhone.Web.Models.Entities;

namespace QDPhone.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "StaffOrAdmin")]
[Route("admin/brands")]
public class BrandsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public BrandsController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await _db.Brands.OrderBy(x => x.Name).ToListAsync());

    [HttpGet("create")]
    public IActionResult Create() => View();

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> Create(Brand model, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(model);
        if (imageFile != null && imageFile.Length > 0)
            model.ImageUrl = await SaveUploadAsync(imageFile, "brands");
        _db.Brands.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var brand = await _db.Brands.FindAsync(id);
        if (brand == null) return NotFound();
        return View(brand);
    }

    [HttpPost("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, Brand model, IFormFile? imageFile)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        var brand = await _db.Brands.FindAsync(id);
        if (brand == null) return NotFound();
        brand.Name = model.Name;
        brand.Slug = model.Slug;
        if (imageFile != null && imageFile.Length > 0)
            brand.ImageUrl = await SaveUploadAsync(imageFile, "brands");
        else
            brand.ImageUrl = model.ImageUrl;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var brand = await _db.Brands.FindAsync(id);
        if (brand == null) return RedirectToAction(nameof(Index));
        _db.Brands.Remove(brand);
        await _db.SaveChangesAsync();
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

