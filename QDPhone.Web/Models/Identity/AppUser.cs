using Microsoft.AspNetCore.Identity;

namespace QDPhone.Web.Models.Identity;

public class AppUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
