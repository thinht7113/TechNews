using Microsoft.AspNetCore.Identity;
using TechNews.Domain.Entities;
using TechNews.Domain.Enums;

namespace TechNews.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(TechNewsDbContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
        {

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new Role { Name = "Admin" });
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new Role { Name = "User" });
            }


            if (!context.Settings.Any())
            {
                var settings = new List<SystemSetting>
                {
                    new SystemSetting { Key = "SiteTitle", Value = "TechNews", DisplayName = "Tên Website", GroupName = "Chung", Type = "text" },
                    new SystemSetting { Key = "SiteDescription", Value = "Trang tin tức công nghệ hàng đầu Việt Nam", DisplayName = "Mô tả Website", GroupName = "Chung", Type = "textarea" },
                    new SystemSetting { Key = "ContactEmail", Value = "contact@technews.com", DisplayName = "Email liên hệ", GroupName = "Liên hệ", Type = "text" },
                    new SystemSetting { Key = "ContactPhone", Value = "0123.456.789", DisplayName = "Số điện thoại", GroupName = "Liên hệ", Type = "text" },
                    new SystemSetting { Key = "FacebookUrl", Value = "https://facebook.com", DisplayName = "Facebook URL", GroupName = "Mạng xã hội", Type = "text" },
                    new SystemSetting { Key = "YoutubeUrl", Value = "https://youtube.com", DisplayName = "Youtube URL", GroupName = "Mạng xã hội", Type = "text" }
                };
                await context.Settings.AddRangeAsync(settings);
                await context.SaveChangesAsync();
            }

            var adminUser = new User
            {
                UserName = "admin@technews.com",
                Email = "admin@technews.com",
                FullName = "Administrator",
                EmailConfirmed = true
            };

            var user = await userManager.FindByEmailAsync(adminUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(adminUser, "Admin@123");
                user = await userManager.FindByEmailAsync(adminUser.Email);
            }

            if (!await userManager.IsInRoleAsync(user!, "Admin"))
            {
                await userManager.AddToRoleAsync(user!, "Admin");
            }

            var author = await userManager.FindByEmailAsync(adminUser.Email);

            var newsCat = context.Categories.FirstOrDefault(c => c.Slug == "tin-tuc");
            if (newsCat != null)
            {
                newsCat.Name = "Công nghệ";
                newsCat.Slug = "cong-nghe";
                await context.SaveChangesAsync();
            }

            var categories = new List<Category>
            {
                new Category { Name = "Công nghệ", Slug = "cong-nghe", Description = "Tin tức công nghệ mới nhất" },
                new Category { Name = "Lập trình", Slug = "lap-trinh", Description = "Chia sẻ kiến thức lập trình" },
                new Category { Name = "Review", Slug = "review", Description = "Đánh giá sản phẩm công nghệ" },
                new Category { Name = "Startup", Slug = "startup", Description = "Câu chuyện khởi nghiệp" },
                new Category { Name = "Kinh doanh", Slug = "kinh-doanh", Description = "Tin tức kinh doanh công nghệ" },
                new Category { Name = "Thủ thuật", Slug = "thu-thuat", Description = "Mẹo hay và hướng dẫn sử dụng" },
                new Category { Name = "Video", Slug = "video", Description = "Video công nghệ và hướng dẫn" }
            };

            foreach (var cat in categories)
            {
                if (!context.Categories.Any(c => c.Slug == cat.Slug))
                {
                    await context.Categories.AddAsync(cat);
                }
            }
            await context.SaveChangesAsync();

            if (!context.MenuItems.Any())
            {
                var menuItems = new List<MenuItem>
                {
                    new MenuItem { Title = "Trang chủ", Url = "/", Order = 0 },
                    new MenuItem { Title = "Công nghệ", Url = "/category/cong-nghe", Order = 1 },
                    new MenuItem { Title = "Lập trình", Url = "/category/lap-trinh", Order = 2 },
                    new MenuItem { Title = "Review", Url = "/category/review", Order = 3 },
                    new MenuItem { Title = "Thủ thuật", Url = "/category/thu-thuat", Order = 4 },
                    new MenuItem { Title = "Video", Url = "/category/video", Order = 5 },
                    new MenuItem { Title = "Liên hệ", Url = "/Home/Contact", Order = 99 }
                };
                await context.MenuItems.AddRangeAsync(menuItems);
                await context.SaveChangesAsync();
            }

            if (!context.Posts.Any())
            {
                var allCats = context.Categories.ToList();
                var posts = new List<Post>();
                var random = new Random();
                var techCat = allCats.First(c => c.Slug == "cong-nghe");
                var devCat = allCats.First(c => c.Slug == "lap-trinh");
                var reviewCat = allCats.First(c => c.Slug == "review");
                var startupCat = allCats.FirstOrDefault(c => c.Slug == "startup") ?? allCats.First();

                posts.Add(new Post
                {
                    Title = "iPhone 16 rò rỉ thiết kế mới với cụm camera dọc",
                    Slug = "iphone-16-ro-ri-thiet-ke-moi",
                    ShortDescription = "Những hình ảnh mới nhất về iPhone 16 cho thấy Apple sẽ quay trở lại thiết kế cụm camera dọc để hỗ trợ quay video không gian.",
                    Content = "<p>Theo các nguồn tin uy tín, iPhone 16 sẽ có sự thay đổi lớn về thiết kế...</p>",
                    Thumbnail = "https://images.unsplash.com/photo-1695046200234-58a436573c7f?auto=format&fit=crop&q=80&w=800",
                    CategoryId = techCat.Id,
                    Status = PostStatus.Published,
                    ViewCount = 1200,
                    Tags = "iphone,apple,cong-nghe",
                    CreatedDate = DateTime.Now.AddDays(-2),
                    AuthorId = author.Id
                });

                posts.Add(new Post
                {
                    Title = "Windows 12 sẽ tích hợp AI sâu vào hệ điều hành",
                    Slug = "windows-12-tich-hop-ai",
                    ShortDescription = "Microsoft đang đẩy mạnh việc tích hợp trí tuệ nhân tạo vào phiên bản Windows tiếp theo, hứa hẹn thay đổi cách chúng ta sử dụng máy tính.",
                    Content = "<p>Windows 12 dự kiến sẽ ra mắt vào năm 2025 với trọng tâm là AI...</p>",
                    Thumbnail = "https://images.unsplash.com/photo-1542831371-29b0f74f9713?auto=format&fit=crop&q=80&w=800",
                    CategoryId = techCat.Id,
                    Status = PostStatus.Published,
                    ViewCount = 850,
                    Tags = "windows,microsoft,ai",
                    CreatedDate = DateTime.Now.AddDays(-5),
                    AuthorId = author.Id
                });

                posts.Add(new Post
                {
                    Title = "Học C# .NET Core từ cơ bản đến nâng cao năm 2024",
                    Slug = "hoc-csharp-net-core-2024",
                    ShortDescription = "Lộ trình học lập trình C# và .NET Core dành cho người mới bắt đầu, cập nhật những công nghệ mới nhất như .NET 8.",
                    Content = "<p>C# vẫn là một trong những ngôn ngữ lập trình phổ biến nhất...</p>",
                    Thumbnail = "https://images.unsplash.com/photo-1599837565318-67429bde8553?auto=format&fit=crop&q=80&w=800",
                    CategoryId = devCat.Id,
                    Status = PostStatus.Published,
                    ViewCount = 3000,
                    Tags = "csharp,dotnet,programming",
                    CreatedDate = DateTime.Now.AddDays(-10),
                    AuthorId = author.Id
                });

                posts.Add(new Post
                {
                    Title = "Tại sao nên dùng Tailwind CSS cho dự án Web?",
                    Slug = "tai-sao-dung-tailwind-css",
                    ShortDescription = "Tailwind CSS giúp tăng tốc độ phát triển giao diện, dễ dàng tùy biến và file CSS đầu ra cực nhẹ.",
                    Content = "<p>Tailwind CSS không chỉ là một framework CSS...</p>",
                    Thumbnail = "https://images.unsplash.com/photo-1587620962725-abab7fe55159?auto=format&fit=crop&q=80&w=800",
                    CategoryId = devCat.Id,
                    Status = PostStatus.Published,
                    ViewCount = 1500,
                    Tags = "css,tailwind,frontend",
                    CreatedDate = DateTime.Now.AddDays(-1),
                    AuthorId = author.Id
                });

                posts.Add(new Post
                {
                    Title = "Đánh giá MacBook Air M3: Vẫn là vua laptop mỏng nhẹ",
                    Slug = "danh-gia-macbook-air-m3",
                    ShortDescription = "Chip M3 mang lại hiệu năng ấn tượng cho dòng MacBook Air, giúp xử lý tốt cả các tác vụ nặng như edit video 4K.",
                    Content = "<p>MacBook Air M3 vẫn giữ nguyên thiết kế...</p>",
                Thumbnail = "https://images.unsplash.com/photo-1611186871348-b1ce696e52c9?auto=format&fit=crop&q=80&w=800",
                    CategoryId = reviewCat.Id,
                    Status = PostStatus.Published,
                    ViewCount = 5000,
                    Tags = "macbook,apple,review",
                    CreatedDate = DateTime.Now.AddDays(-3),
                    AuthorId = author.Id
                });

                posts.Add(new Post
                {
                    Title = "Làn sóng khởi nghiệp AI tại Việt Nam năm 2024",
                    Slug = "khoi-nghiep-ai-viet-nam-2024",
                    ShortDescription = "Các startup công nghệ Việt đang tận dụng làn sóng AI để tạo ra những sản phẩm đột phá, thu hút vốn đầu tư mạnh mẽ.",
                    Content = "<p>Năm 2024 chứng kiến sự bùng nổ của các startup AI...</p>",
                    Thumbnail = "https://images.unsplash.com/photo-1519389950473-47ba0277781c?auto=format&fit=crop&q=80&w=800",
                    CategoryId = startupCat.Id,
                    Status = PostStatus.Published,
                    ViewCount = 2100,
                    Tags = "startup,ai,vietnam,business",
                    CreatedDate = DateTime.Now.AddDays(-4),
                    AuthorId = author.Id
                });

                await context.Posts.AddRangeAsync(posts);
                await context.SaveChangesAsync();
            }
        }
    }
}