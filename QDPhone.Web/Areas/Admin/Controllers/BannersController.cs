using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QDPhone.Web.Data;
using QDPhone.Web.Models.Entities;
using QDPhone.Web.Models.ViewModels;

namespace QDPhone.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "StaffOrAdmin")]
[Route("admin/banners")]
public class BannersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IMemoryCache _cache;

    public BannersController(ApplicationDbContext db, IWebHostEnvironment env, IMemoryCache cache)
    {
        _db = db;
        _env = env;
        _cache = cache;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, bool? isActive, int page = 1, int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 10, 100);
        var query = _db.Banners.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.Title.Contains(q) || x.LinkUrl.Contains(q));
        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);
        var totalItems = await query.CountAsync();
        var rows = await query.OrderByDescending(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        ViewBag.Breadcrumb = "Banner";
        return View(new AdminBannerIndexViewModel
        {
            Rows = rows,
            Q = q,
            IsActive = isActive,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        });
    }

    [HttpGet("create")]
    public IActionResult Create() => View();

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> Create(Banner model, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(model);
        if (imageFile != null && imageFile.Length > 0)
            model.ImageUrl = await SaveUploadAsync(imageFile, "banners");
        _db.Banners.Add(model);
        await _db.SaveChangesAsync();
        _cache.Remove("home-page-vm");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var banner = await _db.Banners.FindAsync(id);
        if (banner == null) return NotFound();
        _db.Banners.Remove(banner);
        await _db.SaveChangesAsync();
        _cache.Remove("home-page-vm");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var banner = await _db.Banners.FindAsync(id);
        if (banner == null) return NotFound();
        return View(banner);
    }

    [HttpPost("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, Banner model, IFormFile? imageFile)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        var banner = await _db.Banners.FindAsync(id);
        if (banner == null) return NotFound();
        banner.Title = model.Title;
        if (imageFile != null && imageFile.Length > 0)
            banner.ImageUrl = await SaveUploadAsync(imageFile, "banners");
        else
            banner.ImageUrl = model.ImageUrl;
        banner.LinkUrl = model.LinkUrl;
        banner.IsActive = model.IsActive;
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

