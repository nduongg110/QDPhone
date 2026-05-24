using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Data;
using QDPhone.Web.Models.Identity;
using QDPhone.Web.Models.ViewModels;

namespace QDPhone.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
[Route("admin/users")]
public class AdminUsersController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;

    public AdminUsersController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? keyword, string? role, int page = 1, int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 10, 100);

        var rowsQuery =
            from user in _userManager.Users
            join userRole in _db.UserRoles on user.Id equals userRole.UserId into userRoles
            from ur in userRoles.DefaultIfEmpty()
            join roleDef in _db.Roles on ur.RoleId equals roleDef.Id into roleDefs
            from rd in roleDefs.DefaultIfEmpty()
            select new AdminUserRowViewModel
            {
                User = user,
                Role = rd != null ? rd.Name! : "User"
            };

        IQueryable<AdminUserRowViewModel> query = rowsQuery;
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLowerInvariant();
            query = query.Where(x => (x.User.Email ?? string.Empty).ToLower().Contains(kw) ||
                                     (x.User.FullName ?? string.Empty).ToLower().Contains(kw));
        }
        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(x => x.Role == role);

        var totalItems = await query.CountAsync();
        var pageRows = await query
            .OrderByDescending(x => x.User.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return View(new AdminUserIndexViewModel
        {
            Rows = pageRows,
            Keyword = keyword,
            Role = role,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        });
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new AdminUserFormViewModel { IsActive = true, Role = "User" });

    [HttpPost("create")]
    public async Task<IActionResult> Create(AdminUserFormViewModel model)
    {
        if (!await ValidateRoleAsync(model.Role))
            ModelState.AddModelError(nameof(model.Role), "Vai trò không hợp lệ.");
        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError(nameof(model.Password), "Mật khẩu không được để trống.");
        if (!ModelState.IsValid) return View(model);

        var user = new AppUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            FullName = model.FullName.Trim(),
            PhoneNumber = model.PhoneNumber,
            Address = model.Address,
            IsActive = model.IsActive,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password!);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, model.Role);
        await LogAuditAsync("CreateUser", "User", user.Id, $"Role={model.Role}");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User";
        return View(new AdminUserFormViewModel
        {
            Id = user.Id,
            FullName = user.FullName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            IsActive = user.IsActive,
            Role = role
        });
    }

    [HttpPost("edit/{id}")]
    public async Task<IActionResult> Edit(string id, AdminUserFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!await ValidateRoleAsync(model.Role))
            ModelState.AddModelError(nameof(model.Role), "Vai trò không hợp lệ.");
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.FullName = model.FullName.Trim();
        user.Email = model.Email.Trim();
        user.UserName = model.Email.Trim();
        user.PhoneNumber = model.PhoneNumber;
        user.Address = model.Address;
        user.IsActive = model.IsActive;
        user.EmailConfirmed = true;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, model.Role);

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
            if (!resetResult.Succeeded)
            {
                foreach (var error in resetResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }
        }

        await LogAuditAsync("EditUser", "User", user.Id, $"Role={model.Role};IsActive={model.IsActive}");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(string userId)
    {
        var actorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.Equals(actorId, userId, StringComparison.Ordinal))
        {
            TempData["Message"] = "Không thể xóa chính tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return RedirectToAction(nameof(Index));

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            TempData["Message"] = "Không thể xóa user do có ràng buộc dữ liệu.";
            return RedirectToAction(nameof(Index));
        }

        await LogAuditAsync("DeleteUser", "User", userId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("toggle-active")]
    public async Task<IActionResult> ToggleActive(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return RedirectToAction(nameof(Index));
        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        await LogAuditAsync("ToggleUserActive", "User", user.Id, $"IsActive={user.IsActive}");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("change-role")]
    public async Task<IActionResult> ChangeRole(string userId, string role)
    {
        if (!await ValidateRoleAsync(role)) return RedirectToAction(nameof(Index));
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return RedirectToAction(nameof(Index));

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, role);
        await LogAuditAsync("ChangeUserRole", "User", user.Id, $"Role={role}");
        return RedirectToAction(nameof(Index));
    }

    private Task<bool> ValidateRoleAsync(string role)
        => _roleManager.RoleExistsAsync(role);

    private async Task LogAuditAsync(string action, string targetType, string targetId, string? detail = null)
    {
        var actorUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        _db.AdminAuditLogs.Add(new QDPhone.Web.Models.Entities.AdminAuditLog
        {
            ActorUserId = actorUserId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Detail = detail
        });
        await _db.SaveChangesAsync();
    }
}

