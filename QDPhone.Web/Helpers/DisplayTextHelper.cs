namespace QDPhone.Web.Helpers;

public static class DisplayTextHelper
{
    public static string OrderStatus(string status) => status switch
    {
        "Pending" => "Chờ xử lý",
        "PendingPayment" => "Chờ thanh toán",
        "Paid" => "Đã thanh toán",
        "Shipping" => "Đang giao",
        "Done" => "Hoàn tất",
        "Cancelled" => "Đã hủy",
        "PaymentFailed" => "Thanh toán thất bại",
        _ => status
    };

    public static string PaymentMethod(string method) => method switch
    {
        "COD" => "Thanh toán khi nhận hàng (COD)",
        "PAYOS" => "PayOS",
        _ => method
    };
}
