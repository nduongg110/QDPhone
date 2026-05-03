using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Data;
using QDPhone.Web.Models.Entities;

namespace QDPhone.Web.Services;

public interface INotificationService
{
    Task NotifyOrderCreatedAsync(string userId, int orderId, decimal amount);
    Task NotifyOrderStatusChangedAsync(string userId, int orderId, string status);
}

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;

    public NotificationService(ApplicationDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public async Task NotifyOrderCreatedAsync(string userId, int orderId, decimal amount)
    {
        var content = $"Đơn hàng #{orderId} đã được tạo thành công. Tổng tiền: {amount:N0} VND.";
        _db.Notifications.Add(new Notification { UserId = userId, Content = content, IsRead = false });
        await _db.SaveChangesAsync();
        var email = await _db.Users.Where(x => x.Id == userId).Select(x => x.Email).FirstOrDefaultAsync();
        if (!string.IsNullOrWhiteSpace(email))
            await _emailService.SendAsync(email, $"Xác nhận đơn hàng #{orderId}", $"<p>{content}</p>");
    }

    public async Task NotifyOrderStatusChangedAsync(string userId, int orderId, string status)
    {
        var content = $"Trạng thái đơn hàng #{orderId} đã cập nhật: {status}.";
        _db.Notifications.Add(new Notification { UserId = userId, Content = content, IsRead = false });
        await _db.SaveChangesAsync();
        var email = await _db.Users.Where(x => x.Id == userId).Select(x => x.Email).FirstOrDefaultAsync();
        if (!string.IsNullOrWhiteSpace(email))
            await _emailService.SendAsync(email, $"Cập nhật đơn hàng #{orderId}", $"<p>{content}</p>");
    }
}
