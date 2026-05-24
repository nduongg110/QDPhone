using QDPhone.Web.Models.Identity;

namespace QDPhone.Web.Models.ViewModels;

public class AdminUserRowViewModel
{
    public AppUser User { get; set; } = new();
    public string Role { get; set; } = "User";
}
