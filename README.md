# 📰 Project-News — Nền tảng tin tức công nghệ

Nền tảng quản lý và xuất bản tin tức công nghệ được xây dựng bằng **ASP.NET Core 9**, sử dụng kiến trúc **Clean Architecture** với giao diện hiện đại và hệ thống quản trị mạnh mẽ.

---

## 🏗️ Kiến trúc dự án

```
Project-News/
├── TechNews.Domain/           # Entities, Enums, Interfaces (Domain Layer)
├── TechNews.Application/      # Services, Business Logic (Application Layer)
├── TechNews.Infrastructure/   # EF Core, Repositories, Data Access (Infra Layer)
└── Project-News/              # ASP.NET Core Web App (Presentation Layer)
    ├── Areas/Admin/            # Admin Panel (Vue.js SPA)
    ├── Controllers/            # Public-facing controllers
    ├── ViewComponents/         # Dynamic UI components
    ├── Views/                  # Razor Views
    └── wwwroot/                # Static files (JS, CSS, Images)
```

### Nguyên tắc thiết kế
- **Clean Architecture** — Tách biệt Domain, Application, Infrastructure, Presentation
- **Repository Pattern + Unit of Work** — Truy xuất dữ liệu linh hoạt
- **Service Layer** — `PostService`, `CategoryService`, `CommentService`, `EmailService`
- **Dependency Injection** — Toàn bộ services và repositories qua DI container

---

## ✨ Tính năng chính

### 🌐 Trang người dùng (Public)
- **Trang chủ** — Bento Grid Layout với bài nổi bật, mới nhất, phân theo danh mục
- **Chi tiết bài viết** — Typography đẹp, tags, bài viết liên quan, bình luận HTMX
- **Tìm kiếm** — Tìm kiếm bài viết theo từ khóa
- **Danh mục & Tags** — Duyệt bài viết theo chuyên mục và thẻ
- **Bình luận** — Đăng nhập và bình luận trực tiếp (HTMX, không reload)
- **Đăng ký Newsletter** — Form đăng ký nhận tin ở footer, lưu vào database
- **Liên hệ** — Trang liên hệ với form gửi tin nhắn
- **Cấu hình động** — Tên website, mô tả, social links... đều lấy từ database

### 🔧 Admin Panel (Vue.js SPA)
- **Dashboard** — Thống kê tổng quan (bài viết, người dùng, bình luận)
- **Quản lý bài viết** — CRUD, hỗ trợ CKEditor, upload ảnh, draft/publish, thùng rác
- **Quản lý danh mục** — Tạo, sửa, xóa chuyên mục
- **Quản lý Tags** — Gắn thẻ cho bài viết
- **Quản lý người dùng** — CRUD, phân quyền Admin/User
- **Quản lý bình luận** — Duyệt, xóa, trả lời bình luận
- **Thư viện Media** — Upload và quản lý ảnh
- **Menu Builder** — Xây dựng menu navigation động
- **Cấu hình hệ thống** — Quản lý cài đặt (tên site, liên hệ, mạng xã hội, SMTP)
- **Newsletter** — Quản lý subscriber + soạn & gửi newsletter email
- **Bảo trì & Logs** — Quản lý hệ thống

### 🔒 Bảo mật & Hiệu suất
- **ASP.NET Core Identity** — Đăng ký, đăng nhập, phân quyền (Admin/User)
- **Global Exception Middleware** — Xử lý lỗi toàn cục (JSON cho API, redirect cho MVC)
- **Custom Error Pages** — Trang lỗi 403, 404, 500 thiết kế đẹp
- **Input Validation** — Data Annotations + ModelState validation
- **Memory Caching** — Cache trang chủ 5 phút giảm tải database
- **Server-side Pagination** — Phân trang API cho Admin (Posts, Users, Comments)

---

## 🛠️ Công nghệ sử dụng

| Layer | Công nghệ |
|---|---|
| **Backend** | ASP.NET Core 9, C# |
| **Database** | SQL Server + Entity Framework Core 9 |
| **Authentication** | ASP.NET Core Identity |
| **Admin Frontend** | Vue.js 3 (SPA), SweetAlert2 |
| **Public Frontend** | Razor Views, Tailwind CSS, Alpine.js, HTMX |
| **Editor** | CKEditor 5 |
| **Fonts** | Google Fonts (Outfit, Merriweather) |
| **Icons** | Bootstrap Icons |
| **Email** | System.Net.Mail (SMTP) |
| **Caching** | IMemoryCache |

---

## 🚀 Hướng dẫn chạy

### Yêu cầu
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB hoặc Express)

### Cài đặt

```bash
# 1. Clone repository
git clone <repo-url>
cd Project-News

# 2. Cập nhật connection string trong appsettings.json
#    Mở Project-News/appsettings.json và sửa "DefaultConnection"

# 3. Tạo database
dotnet ef database update --project TechNews.Infrastructure --startup-project Project-News

# 4. Chạy ứng dụng
dotnet run --project Project-News
```

### Truy cập
| URL | Mô tả |
|---|---|
| `https://localhost:7289` | Trang người dùng |
| `https://localhost:7289/Admin` | Admin Panel |

### Tài khoản mặc định
- **Admin:** `admin@technews.com` / `Admin@123`

---

## ⚙️ Cấu hình

### Database (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=TechNewsDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### Email/Newsletter (Cấu hình trong Admin → Cấu hình)
Cấu hình SMTP trực tiếp từ giao diện admin:
- **SMTP Host** — `smtp.gmail.com`
- **SMTP Port** — `587`
- **Email gửi** — Gmail của bạn
- **Mật khẩu ứng dụng** — [App Password](https://myaccount.google.com/apppasswords)

---

## 📁 Cấu trúc chính

### Domain Entities
`Post` · `Category` · `Tag` · `PostTag` · `Comment` · `User` · `Role` · `MenuItem` · `Contact` · `SystemSetting` · `PostRevision` · `Subscriber`

### Admin API Endpoints
| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/post/getall` | Danh sách bài viết (phân trang) |
| POST | `/api/post/create` | Tạo bài viết mới |
| GET | `/api/category/getall` | Danh sách danh mục |
| GET | `/api/user/getall` | Danh sách người dùng (phân trang) |
| GET | `/api/comment/getall` | Danh sách bình luận (phân trang) |
| GET | `/api/setting/getall` | Cấu hình hệ thống |
| POST | `/api/setting/update` | Cập nhật cấu hình |
| GET | `/api/newsletter/subscribers` | Danh sách subscriber |
| POST | `/api/newsletter/send` | Gửi newsletter |
| POST | `/api/newsletter/subscribe` | Đăng ký nhận tin |

---

## 👤 Tác giả

**Hoàng Đức Thịnh**

---

*Được phát triển bằng ❤️ với ASP.NET Core & Vue.js*
