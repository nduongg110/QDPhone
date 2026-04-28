using System.ComponentModel.DataAnnotations;

namespace QDPhone.Web.Models.ViewModels;

public class AdminUserFormViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Họ tên không được để trống.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "Vai trò không được để trống.")]
    public string Role { get; set; } = "User";

    [MinLength(8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự.")]
    public string? Password { get; set; }
}
