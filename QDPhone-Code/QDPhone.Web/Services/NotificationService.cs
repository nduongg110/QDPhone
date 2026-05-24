using QDPhone.Web.Data;
using QDPhone.Web.Models.Entities;

namespace QDPhone.Web.Services;

public interface INotificationService
{
    Task NotifyOrderCreatedAsync(string userId, int orderId, decimal amount, IList<OrderItem>? items = null);
    Task NotifyOrderStatusChangedAsync(string userId, int orderId, string status);
}

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;

    public NotificationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task NotifyOrderCreatedAsync(string userId, int orderId, decimal amount, IList<OrderItem>? items = null)
    {
        string itemsSummary = string.Empty;
        if (items != null && items.Count > 0)
        {
            itemsSummary = " bao gồm " + string.Join(", ", items.Select(i => $"{i.ProductName} ({i.Quantity})"));
        }

        var content = $"Đơn hàng #{orderId}{itemsSummary} đã được tạo thành công. Tổng tiền: {amount:N0} VND.";
        _db.Notifications.Add(new Notification { UserId = userId, Content = content, IsRead = false });
        await _db.SaveChangesAsync();
    }

    public async Task NotifyOrderStatusChangedAsync(string userId, int orderId, string status)
    {
        var content = $"Trạng thái đơn hàng #{orderId} đã cập nhật: {status}.";
        _db.Notifications.Add(new Notification { UserId = userId, Content = content, IsRead = false });
        await _db.SaveChangesAsync();
    }
}