QDPhone là một dự án web bán điện thoại trực tuyến, được xây dựng với **ReactJS** cho frontend và **NodeJS** cho backend. Dự án sử dụng **SQL Server** làm hệ quản trị cơ sở dữ liệu để lưu trữ thông tin sản phẩm, người dùng và đơn hàng.

 Công nghệ sử dụng
- **ReactJS**: Xây dựng giao diện người dùng, hỗ trợ SPA (Single Page Application).
- **NodeJS & Express**: Xây dựng API backend, xử lý logic và kết nối cơ sở dữ liệu.
- **SQL Server**: Lưu trữ dữ liệu sản phẩm, người dùng và đơn hàng.
- **JWT Authentication**: Bảo mật đăng nhập và quản lý phiên người dùng.

 Cài đặt và chạy dự án

 1. Clone repository
```bash
git clone https://github.com/nduongg110/QDPhone.git
cd QDPhone
```

 2. Cài đặt dependencies
```bash
npm install
```

 3. Cấu hình SQL Server
- Tạo database `QDPhoneDB` trong SQL Server.
- Import các bảng: `Users`, `Products`, `Orders`, `Cart`.
- Cập nhật thông tin kết nối trong file `server/config/db.js`:
```js
const sql = require('mssql');

const config = {
  user: 'your_username',
  password: 'your_password',
  server: 'localhost',
  database: 'QDPhoneDB',
  options: {
    encrypt: true,
    trustServerCertificate: true
  }
};

module.exports = config;
```

 4. Chạy server backend
```bash
cd server
npm start
```

 5. Chạy frontend React
```bash
cd client
npm start
```

Ứng dụng sẽ chạy tại `http://localhost:3000`.

 Cấu trúc thư mục
```
QDPhone/
│── client/        # ReactJS frontend
│── server/        # NodeJS backend
│── README.md      # Tài liệu dự án
```

 Tính năng chính
- Hiển thị danh sách điện thoại theo danh mục.
- Tìm kiếm và lọc sản phẩm.
- Đăng ký/đăng nhập người dùng.
- Quản lý giỏ hàng và đặt hàng.
- Trang quản trị để thêm/sửa/xóa sản phẩm.
