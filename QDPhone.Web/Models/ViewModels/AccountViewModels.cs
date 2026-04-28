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
