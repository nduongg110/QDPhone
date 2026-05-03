using System.ComponentModel.DataAnnotations;

namespace QDPhone.Web.Models.ViewModels;

public class RegisterViewModel
{
    [Required, Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Display(Name = "Xác nhận mật khẩu"), Compare(nameof(Password), ErrorMessage = "Xác nhận mật khẩu không khớp.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginViewModel
{
    [Required, EmailAddress, Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;
    [Display(Name = "Nhớ đăng nhập")]
    public bool RememberMe { get; set; }
}

public class ProfileViewModel
{
    [Required, Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;
    [Phone, Display(Name = "Số điện thoại")]
    public string? PhoneNumber { get; set; }
    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
}

public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password), Display(Name = "Mật khẩu hiện tại")]
    public string CurrentPassword { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Display(Name = "Mật khẩu mới")]
    public string NewPassword { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Display(Name = "Xác nhận mật khẩu mới"), Compare(nameof(NewPassword), ErrorMessage = "Xác nhận mật khẩu mới không khớp.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class ForgotPasswordViewModel
{
    [Required, EmailAddress, Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required, EmailAddress, Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Token { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Display(Name = "Mật khẩu mới")]
    public string NewPassword { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Display(Name = "Xác nhận mật khẩu mới"), Compare(nameof(NewPassword), ErrorMessage = "Xác nhận mật khẩu mới không khớp.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
