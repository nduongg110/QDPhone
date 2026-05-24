namespace QDPhone.Web.Models.ViewModels;

public class AdminDashboardViewModel
{
    public decimal TodayRevenue { get; set; }
    public decimal RevenueChangePercent { get; set; }
    public int NewOrdersCount { get; set; }
    public int PendingOrdersCount { get; set; }
    public int NewUsersToday { get; set; }
    public int TotalUsers { get; set; }
    public int LowStockCount { get; set; }
    public int TotalOrders { get; set; }

    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalBrands { get; set; }
    public int PendingReviews { get; set; }
    public int ActiveCoupons { get; set; }
    public int ActiveBanners { get; set; }

    public List<DashboardRevenuePointViewModel> RevenueLast30Days { get; set; } = new();
    public List<DashboardStatusPointViewModel> OrderStatusBreakdown { get; set; } = new();
    public List<DashboardRecentOrderViewModel> RecentOrders { get; set; } = new();
    public List<DashboardTopProductViewModel> TopProducts { get; set; } = new();
    public List<DashboardActivityViewModel> Activities { get; set; } = new();
}

public class DashboardRevenuePointViewModel
{
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}

public class DashboardStatusPointViewModel
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DashboardRecentOrderViewModel
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DashboardTopProductViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public int SoldQuantity { get; set; }
    public decimal Revenue { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}

public class DashboardActivityViewModel
{
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime At { get; set; }
}
