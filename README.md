# 📰 TechNews — Content Marketing & AI Platform

Nền tảng quản lý tin tức công nghệ tích hợp **AI Writing Assistant**, **SEO Toolkit**, **Content Analytics** và **Editorial Workflow** — xây dựng bằng **ASP.NET Core 9** với kiến trúc **Clean Architecture**.

---

## 🏗️ Kiến trúc dự án

```
TechNews/
├── TechNews.Domain/           # Entities, Enums, Interfaces
├── TechNews.Application/      # Services, DTOs, Business Logic
├── TechNews.Infrastructure/   # EF Core, Repositories, Data Access
└── TechNews.Web/              # ASP.NET Core Web App
    ├── Areas/Admin/            # Admin Panel (Vue.js SPA)
    ├── Controllers/            # Public-facing controllers
    ├── Services/               # Background Services
    ├── ViewComponents/         # Dynamic UI components
    ├── Views/                  # Razor Views (Public)
    └── wwwroot/js/admin/       # Vue.js Components
        ├── components/
        │   ├── post/           # PostForm, PostList, PostTrash
        │   ├── Dashboard.js
        │   ├── Workflow.js     # Editorial workflow
        │   ├── Analytics.js    # Content analytics
        │   └── ContentCalendar.js
        └── spa-app.js          # Vue Router
```

### Nguyên tắc thiết kế
- **Clean Architecture** — Domain → Application → Infrastructure → Presentation
- **Repository Pattern** — Generic `IRepository<T>` cho mọi entity
- **Service Layer** — Business logic tách biệt khỏi controllers
- **Dependency Injection** — Toàn bộ services qua DI container

---

## ✨ Tính năng

### 🌐 Trang công khai
| Tính năng | Mô tả |
|---|---|
| Trang chủ | Bento Grid Layout, bài nổi bật, phân theo danh mục |
| Bài viết | Typography đẹp, tags, bài liên quan, bình luận HTMX |
| Tìm kiếm | Full-text search theo từ khóa |
| Danh mục & Tags | Duyệt bài theo chuyên mục và thẻ |
| Newsletter | Đăng ký nhận tin, gửi email hàng loạt |
| Cấu hình động | Tên site, mô tả, social links từ database |

### 🔧 Admin Panel (Vue.js SPA)
| Tính năng | Mô tả |
|---|---|
| Dashboard | Thống kê tổng quan với biểu đồ |
| Bài viết | CRUD, CKEditor 5, upload ảnh, draft/publish, lịch sử phiên bản |
| Danh mục & Tags | Quản lý chuyên mục và thẻ |
| Người dùng | CRUD, phân quyền Admin/Editor/User |
| Bình luận | Duyệt, xóa, trả lời |
| Thư viện Media | Upload và quản lý ảnh |
| Menu Builder | Menu navigation động |
| Cấu hình | Cài đặt hệ thống, SMTP, AI Provider |
| Newsletter | Quản lý subscriber, soạn & gửi email |
| Bảo trì & Logs | Quản lý hệ thống |

### 🤖 AI Writing Assistant
| Tính năng | Mô tả |
|---|---|
| Sinh nội dung | Nhập chủ đề → AI viết bài hoàn chỉnh |
| Gợi ý tiêu đề | 5 tiêu đề SEO-friendly cho bài viết |
| Cải thiện văn phong | AI rewrite nội dung chuyên nghiệp hơn |
| Tóm tắt tự động | Tạo short description từ nội dung |
| Gợi ý tags | Phân tích nội dung → đề xuất tags phù hợp |
| Hỗ trợ đa provider | OpenAI (GPT) & Google Gemini |

> AI Panel nằm bên phải trang viết bài, toggle on/off. Nội dung Markdown từ AI tự động convert sang HTML cho CKEditor.

### 📊 SEO Toolkit
| Tính năng | Mô tả |
|---|---|
| Focus Keyword | Nhập từ khóa chính để phân tích |
| Phân tích 6 chiều | Title, Meta Description, Keyword Density, Readability, Headings, Images |
| Realtime Score | Điểm SEO 0-100 với color indicator |
| Gợi ý cải thiện | Danh sách suggestions cụ thể bằng tiếng Việt |

### ✅ Editorial Workflow
| Tính năng | Mô tả |
|---|---|
| Submit → Review | Tác giả gửi bài, Editor duyệt |
| Approve / Reject | Duyệt hoặc từ chối với ghi chú |
| Schedule | Lên lịch xuất bản tự động |
| Audit Log | Lịch sử workflow cho từng bài viết |
| RBAC | Phân quyền Admin / Editor / User |

### 📈 Content Analytics
| Tính năng | Mô tả |
|---|---|
| Daily Views Chart | Biểu đồ lượt xem theo ngày (Chart.js) |
| Stat Cards | Tổng views, unique visitors, avg time, bounce rate |
| Top Posts | Bài viết được xem nhiều nhất |
| Top Referrers | Nguồn truy cập hàng đầu |
| Time Range | Lọc theo 7/14/30/90 ngày |

### 📅 Content Calendar
| Tính năng | Mô tả |
|---|---|
| Monthly Grid | Lịch tháng hiển thị bài viết |
| Color-coded | Màu sắc theo trạng thái (Published/Scheduled/Draft/Rejected) |
| Schedule Dialog | Lên lịch xuất bản mới từ calendar |
| Auto-publish | BackgroundService tự động publish khi đến giờ |

---

## 🛠️ Công nghệ

| Layer | Công nghệ |
|---|---|
| **Backend** | ASP.NET Core 9, C# |
| **Database** | SQL Server + Entity Framework Core 9 |
| **Auth** | ASP.NET Core Identity (RBAC) |
| **Admin UI** | Vue.js 3 (SPA), Vue Router 4, SweetAlert2 |
| **Public UI** | Razor Views, Tailwind CSS, Alpine.js, HTMX |
| **Editor** | CKEditor 5 |
| **Charts** | Chart.js |
| **AI** | OpenAI API / Google Gemini API |
| **Email** | System.Net.Mail (SMTP) |
| **Logging** | Serilog |
| **Caching** | IMemoryCache |

---

## 🚀 Cài đặt & Chạy

### Yêu cầu
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB hoặc Express)
- (Tùy chọn) OpenAI API Key hoặc Google Gemini API Key

### Cài đặt

```bash
# 1. Clone repository
git clone <repo-url>
cd TechNews

# 2. Cập nhật connection string
#    Mở TechNews.Web/appsettings.json → sửa "DefaultConnection"

# 3. Tạo database + apply migrations
dotnet ef database update --project TechNews.Infrastructure --startup-project TechNews.Web

# 4. Chạy ứng dụng
dotnet run --project TechNews.Web
```

### Truy cập
| URL | Mô tả |
|---|---|
| `https://localhost:7289` | Trang công khai |
| `https://localhost:7289/Admin` | Admin Panel |

### Tài khoản mặc định
- **Admin:** `admin@technews.com` / `Admin@123`

---

## ⚙️ Cấu hình

### Database
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=TechNewsDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### AI (Admin → Cấu hình)
| Setting | Giá trị |
|---|---|
| `AiProvider` | `OpenAI` hoặc `Gemini` |
| `AiApiKey` | API key của bạn |
| `AiModel` | `gpt-4o-mini`, `gemini-2.0-flash`, etc. |

### Email/SMTP (Admin → Cấu hình)
| Setting | Ví dụ |
|---|---|
| SMTP Host | `smtp.gmail.com` |
| SMTP Port | `587` |
| Email | Gmail của bạn |
| Password | [App Password](https://myaccount.google.com/apppasswords) |

---

## � API Endpoints

### Core APIs
| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/post/getall` | Danh sách bài viết (phân trang) |
| POST | `/api/post/create` | Tạo bài viết |
| GET | `/api/category/getall` | Danh sách danh mục |
| GET | `/api/user/getall` | Danh sách người dùng |
| GET | `/api/comment/getall` | Danh sách bình luận |

### AI APIs
| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/ai/status` | Kiểm tra AI đã cấu hình chưa |
| POST | `/api/ai/generate` | Sinh nội dung từ prompt |
| POST | `/api/ai/summarize` | Tóm tắt nội dung |
| POST | `/api/ai/suggest-tags` | Gợi ý tags |
| POST | `/api/ai/improve` | Cải thiện văn phong |
| POST | `/api/ai/suggest-titles` | Gợi ý tiêu đề |

### SEO & Analytics
| Method | Endpoint | Mô tả |
|---|---|---|
| POST | `/api/seo/analyze` | Phân tích SEO (6 chiều) |
| POST | `/api/analytics/track` | Tracking lượt xem (public) |
| GET | `/api/analytics/overview` | Tổng quan analytics |

### Workflow & Calendar
| Method | Endpoint | Mô tả |
|---|---|---|
| POST | `/api/workflow/submit/{id}` | Gửi bài đi duyệt |
| POST | `/api/workflow/approve/{id}` | Duyệt bài |
| POST | `/api/workflow/reject/{id}` | Từ chối bài |
| GET | `/api/calendar/events` | Events cho calendar |
| POST | `/api/calendar/schedule` | Lên lịch xuất bản |

---

## 📁 Domain Entities

`Post` · `Category` · `Tag` · `PostTag` · `Comment` · `User` · `Role` · `MenuItem` · `Contact` · `SystemSetting` · `PostRevision` · `Subscriber` · `PageView` · `WorkflowLog`

---

## 🔒 Bảo mật & Hiệu suất
- **ASP.NET Core Identity** — RBAC (Admin / Editor / User)
- **Global Exception Middleware** — JSON cho API, redirect cho MVC
- **Custom Error Pages** — 403, 404, 500
- **Memory Caching** — Cache trang chủ 5 phút + AI settings 5 phút
- **Server-side Pagination** — Phân trang mọi danh sách
- **Background Service** — Auto-publish scheduled posts

---

## 👤 Tác giả

**Hoàng Đức Thịnh**

---

*Được phát triển bằng ❤️ với ASP.NET Core, Vue.js & AI*
