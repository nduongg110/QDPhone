using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QDPhone.Web.Models.Identity;
using QDPhone.Web.Models.ViewModels;
using System.Security.Claims;

namespace QDPhone.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IWebHostEnvironment environment,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _environment = environment;
        _logger = logger;
    }

    //REGISTER
    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "User");

        return RedirectToAction(nameof(RegisterSuccess));
    }

    public IActionResult RegisterSuccess() => View();

    //LOGIN
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            true
        );

        if (result.Succeeded)
            return RedirectToAction("Index", "Home");

        ModelState.AddModelError(string.Empty, "Sai tài khoản hoặc mật khẩu.");
        return View(model);
    }

    //LOGOUT
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    //EXTERNAL LOGIN
    [HttpPost]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null) return RedirectToAction(nameof(Login));

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null) return RedirectToAction(nameof(Login));

        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            false,
            true
        );

        if (result.Succeeded)
            return Redirect(returnUrl ?? "/");

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
            return RedirectToAction(nameof(Login));

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = info.Principal.Identity?.Name ?? email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return RedirectToAction(nameof(Login));

            await _userManager.AddToRoleAsync(user, "User");
        }

        var linkResult = await _userManager.AddLoginAsync(user, info);
        if (!linkResult.Succeeded)
            return RedirectToAction(nameof(Login));

        await _signInManager.SignInAsync(user, false);

        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();

    //PROFILE
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        return View(new ProfileViewModel
        {
            FullName = user.FullName ?? "",
            Address = user.Address,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Profile(ProfileViewModel model, IFormFile? avatar)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        user.FullName = model.FullName;
        user.Address = model.Address;
        user.PhoneNumber = model.PhoneNumber;

        if (avatar is { Length: > 0 })
        {
            var extension = Path.GetExtension(avatar.FileName);
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "File ảnh không hợp lệ.");
                return View(model);
            }

            if (avatar.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("", "Ảnh tối đa 2MB.");
                return View(model);
            }

            var fileName = $"{user.Id}_{Guid.NewGuid():N}{extension}";
            var path = Path.Combine(_environment.WebRootPath, "uploads", "avatars");

            Directory.CreateDirectory(path);

            var filePath = Path.Combine(path, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await avatar.CopyToAsync(stream);

            user.AvatarUrl = $"/uploads/avatars/{fileName}";
        }

        await _userManager.UpdateAsync(user);

        TempData["Message"] = "Cập nhật thành công.";
        return RedirectToAction(nameof(Profile));
    }

    //CHANGE PASSWORD
    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword() => View();

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        var result = await _userManager.ChangePasswordAsync(
            user,
            model.CurrentPassword,
            model.NewPassword
        );

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        TempData["Message"] = "Đổi mật khẩu thành công.";
        return RedirectToAction(nameof(Profile));
    }
}