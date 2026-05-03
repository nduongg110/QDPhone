using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QDPhone.Web.Models.Identity;
using QDPhone.Web.Models.ViewModels;
using System.Security.Claims;

namespace QDPhone.Web.Controllers;

// Controller xử lý toàn bộ chức năng liên quan đến tài khoản
public class AccountController : Controller
{
    // Quản lý user (tạo, update, role, password...)
    private readonly UserManager<AppUser> _userManager;

    // Xử lý đăng nhập / đăng xuất
    private readonly SignInManager<AppUser> _signInManager;

    // Lấy thông tin môi trường (đường dẫn wwwroot, upload file...)
    private readonly IWebHostEnvironment _environment;

    // Ghi log (debug, error...)
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

    // ================= REGISTER =================

    // Hiển thị form đăng ký
    [HttpGet]
    public IActionResult Register() => View();

    // Xử lý đăng ký
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Nếu dữ liệu không hợp lệ -> trả lại form
        if (!ModelState.IsValid) return View(model);

        // Tạo user mới
        var user = new AppUser
        {
            UserName = model.Email,       // Username = Email
            Email = model.Email,
            FullName = model.FullName,
            EmailConfirmed = true         // Bỏ qua xác thực email
        };

        // Tạo user trong DB
        var result = await _userManager.CreateAsync(user, model.Password);

        // Nếu lỗi (vd: password yếu, email trùng...)
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // Gán role mặc định là "User"
        await _userManager.AddToRoleAsync(user, "User");

        // Chuyển sang trang đăng ký thành công
        return RedirectToAction(nameof(RegisterSuccess));
    }

    public IActionResult RegisterSuccess() => View();

    // ================= LOGIN =================

    // Hiển thị form login
    [HttpGet]
    public IActionResult Login() => View();

    // Xử lý login
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Thử đăng nhập bằng email + password
        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,  // ghi nhớ đăng nhập
            true               // lock account nếu sai nhiều lần
        );

        // Nếu thành công -> về trang chủ
        if (result.Succeeded)
            return RedirectToAction("Index", "Home");

        // Nếu sai -> báo lỗi
        ModelState.AddModelError(string.Empty, "Sai tài khoản hoặc mật khẩu.");
        return View(model);
    }

    // ================= LOGOUT =================

    [Authorize] // chỉ user đã login mới logout được
    [HttpPost]
    [ValidateAntiForgeryToken] // chống CSRF
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync(); // đăng xuất
        return RedirectToAction("Index", "Home");
    }

    // ================= EXTERNAL LOGIN (Google, Facebook...) =================

    // Gửi request tới provider (Google...)
    [HttpPost]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        // URL callback sau khi login xong
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });

        // Cấu hình thông tin xác thực ngoài
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        // Redirect sang Google/Facebook
        return Challenge(properties, provider);
    }

    // Xử lý callback từ Google/Facebook
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        // Nếu có lỗi từ provider
        if (remoteError != null) return RedirectToAction(nameof(Login));

        // Lấy thông tin login ngoài
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null) return RedirectToAction(nameof(Login));

        // Thử đăng nhập nếu đã liên kết trước đó
        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            false,
            true
        );

        if (result.Succeeded)
            return Redirect(returnUrl ?? "/");

        // Lấy email từ provider
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
            return RedirectToAction(nameof(Login));

        // Kiểm tra user đã tồn tại chưa
        var user = await _userManager.FindByEmailAsync(email);

        // Nếu chưa có -> tạo mới
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

        // Liên kết tài khoản external với user
        var linkResult = await _userManager.AddLoginAsync(user, info);
        if (!linkResult.Succeeded)
            return RedirectToAction(nameof(Login));

        // Đăng nhập
        await _signInManager.SignInAsync(user, false);

        return RedirectToAction("Index", "Home");
    }

    // Trang bị từ chối truy cập (không đủ quyền)
    public IActionResult AccessDenied() => View();

    // ================= PROFILE =================

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        // Lấy user hiện tại
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        // Trả dữ liệu sang View
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

        // Cập nhật thông tin cơ bản
        user.FullName = model.FullName;
        user.Address = model.Address;
        user.PhoneNumber = model.PhoneNumber;

        // Xử lý upload avatar
        if (avatar is { Length: > 0 })
        {
            var extension = Path.GetExtension(avatar.FileName);

            // Chỉ cho phép các định dạng ảnh
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "File ảnh không hợp lệ.");
                return View(model);
            }

            // Giới hạn dung lượng 2MB
            if (avatar.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("", "Ảnh tối đa 2MB.");
                return View(model);
            }

            // Tạo tên file unique
            var fileName = $"{user.Id}_{Guid.NewGuid():N}{extension}";

            // Đường dẫn lưu file
            var path = Path.Combine(_environment.WebRootPath, "uploads", "avatars");

            Directory.CreateDirectory(path);

            var filePath = Path.Combine(path, fileName);

            // Lưu file
            await using var stream = new FileStream(filePath, FileMode.Create);
            await avatar.CopyToAsync(stream);

            // Lưu đường dẫn vào DB
            user.AvatarUrl = $"/uploads/avatars/{fileName}";
        }

        await _userManager.UpdateAsync(user);

        TempData["Message"] = "Cập nhật thành công.";
        return RedirectToAction(nameof(Profile));
    }

    // ================= CHANGE PASSWORD =================

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

        // Đổi mật khẩu
        var result = await _userManager.ChangePasswordAsync(
            user,
            model.CurrentPassword,
            model.NewPassword
        );

        // Nếu lỗi
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