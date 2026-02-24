using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TechNews.Domain.Entities;
using TechNews.Infrastructure.Data;

namespace TechNews.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingController : Controller
    {
        private readonly TechNewsDbContext _context;

        public SettingController(TechNewsDbContext context)
        {
            _context = context;
        }

        [Route("Admin/Settings")]
        public IActionResult Index() => View("Spa");

        [HttpGet]
        [Route("api/setting/getall")]
        public async Task<IActionResult> GetAll()
        {
            var defaultSettings = new List<SystemSetting>
            {
                new SystemSetting { Key = "SiteTitle", Value = "TechNews", DisplayName = "Tên Website", GroupName = "Chung", Type = "text" },
                new SystemSetting { Key = "SiteDescription", Value = "Trang tin tức công nghệ hàng đầu Việt Nam", DisplayName = "Mô tả Website", GroupName = "Chung", Type = "textarea" },
                new SystemSetting { Key = "ContactEmail", Value = "contact@technews.com", DisplayName = "Email liên hệ", GroupName = "Liên hệ", Type = "text" },
                new SystemSetting { Key = "ContactPhone", Value = "0123.456.789", DisplayName = "Số điện thoại", GroupName = "Liên hệ", Type = "text" },
                new SystemSetting { Key = "FacebookUrl", Value = "https://facebook.com", DisplayName = "Facebook URL", GroupName = "Mạng xã hội", Type = "text" },
                new SystemSetting { Key = "YoutubeUrl", Value = "https://youtube.com", DisplayName = "Youtube URL", GroupName = "Mạng xã hội", Type = "text" },
                new SystemSetting { Key = "SmtpHost", Value = "smtp.gmail.com", DisplayName = "SMTP Host", GroupName = "Email (Newsletter)", Type = "text" },
                new SystemSetting { Key = "SmtpPort", Value = "587", DisplayName = "SMTP Port", GroupName = "Email (Newsletter)", Type = "text" },
                new SystemSetting { Key = "SmtpUser", Value = "", DisplayName = "Email gửi (Gmail)", GroupName = "Email (Newsletter)", Type = "text" },
                new SystemSetting { Key = "SmtpPass", Value = "", DisplayName = "Mật khẩu ứng dụng", GroupName = "Email (Newsletter)", Type = "password" },
                new SystemSetting { Key = "SmtpFromName", Value = "TechNews", DisplayName = "Tên người gửi", GroupName = "Email (Newsletter)", Type = "text" }
            };

            var existingKeys = await _context.Settings.Select(s => s.Key).ToListAsync();
            var needsInsert = false;
            foreach (var s in defaultSettings)
            {
                if (!existingKeys.Contains(s.Key))
                {
                    _context.Settings.Add(s);
                    needsInsert = true;
                }
            }
            if (needsInsert) await _context.SaveChangesAsync();

            var settings = await _context.Settings.OrderBy(s => s.GroupName).ThenBy(s => s.Key).ToListAsync();
            return Json(settings);
        }

        [HttpPost]
        [Route("api/setting/update")]
        public async Task<IActionResult> Update([FromBody] List<SystemSetting> model)
        {
            if (model == null || !model.Any()) return BadRequest("No settings provided");

            foreach (var item in model)
            {
                var setting = await _context.Settings.FindAsync(item.Key);
                if (setting != null)
                {
                    setting.Value = item.Value;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}