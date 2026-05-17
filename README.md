QDPhone

Website thương mại điện tử bán điện thoại & phụ kiện, xây dựng bằng ASP.NET Core MVC + Entity Framework Core.

## Tính năng

### Người dùng
- Đăng ký, đăng nhập (Email/Password & Google OAuth)
- Xem sản phẩm, tìm kiếm, lọc theo danh mục & thương hiệu
- Giỏ hàng, danh sách yêu thích
- Đặt hàng & thanh toán qua **PayOS**
- Theo dõi trạng thái đơn hàng
- Đánh giá sản phẩm
- Nhận thông báo

### Quản trị (Admin/Staff)
- Quản lý sản phẩm, danh mục, thương hiệu
- Quản lý đơn hàng & kho hàng
- Quản lý người dùng & phân quyền
- Mã giảm giá (Coupon)
- Xuất báo cáo Excel / PDF

## Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Framework | ASP.NET Core MVC (.NET 10) |
| ORM | Entity Framework Core 10 |
| Database | SQL Server |
| Auth | ASP.NET Core Identity + Google OAuth |
| Thanh toán | PayOS |
| Email | MailKit |
| Xuất file | ClosedXML (Excel), QuestPDF (PDF) 

## Yêu cầu cài đặt

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (hoặc SQL Server Express)
- Tài khoản Google Cloud (để OAuth)
- Tài khoản PayOS (để thanh toán)

---

## Hướng dẫn chạy dự án

### 1. Clone repository

```bash
git clone https://github.com/your-username/QDPhone.git
cd QDPhone
```

### 2. Cấu hình User Secrets

Dự án dùng **User Secrets** để bảo mật thông tin nhạy cảm, không lưu trong code.

```bash
cd QDPhone.Web

dotnet user-secrets set "Authentication:Google:ClientId" "your-google-client-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-google-client-secret"
dotnet user-secrets set "PayOS:ApiKey" "your-payos-api-key"
dotnet user-secrets set "PayOS:ChecksumKey" "your-payos-checksum-key"
dotnet user-secrets set "Email:Username" "your-email@gmail.com"
dotnet user-secrets set "Email:Password" "your-email-app-password"
```

### 3. Cập nhật connection string

Mở file `appsettings.json`, sửa lại connection string cho phù hợp với SQL Server của bạn:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=QDPhoneDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
}
```

### 4. Chạy migration

```bash
dotnet ef database update
```

### 5. Chạy dự án

```bash
dotnet run
```

Truy cập tại: `https://localhost:7010`

## Phân quyền

| Role | Quyền |
|---|---|
| `Admin` | Toàn quyền quản trị |
| `Staff` | Quản lý sản phẩm & đơn hàng |
| _(không có role)_ | Khách hàng thông thường |

Tài khoản Admin cần được tạo thủ công trong database hoặc qua seed data.

## Cấu trúc thư mục

```
QDPhone.Web/
├── Areas/
│   └── Admin/          # Khu vực quản trị
├── Controllers/        # Controllers phía người dùng
├── Models/             # Entity models & ViewModels
├── Views/              # Razor Views
├── Services/           # Business logic
├── Data/               # DbContext & Migrations
└── wwwroot/            # Static files (CSS, JS, ảnh)
```


## License

Dự án được phát triển cho mục đích học tập.
