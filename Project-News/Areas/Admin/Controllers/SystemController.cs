using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TechNews.Infrastructure.Data;
using TechNews.Domain.Entities;
using System.IO;
using System.Text.Json;

namespace Project_News.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SystemController : Controller
    {
        private readonly TechNewsDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SystemController(TechNewsDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [Route("Admin/System")]
        public IActionResult Index() => View("/Areas/Admin/Views/Shared/Spa.cshtml");

        [HttpGet]
        [Route("api/system/backup")]
        public async Task<IActionResult> GetBackup()
        {
            var users = await _context.Users.ToListAsync();
            var posts = await _context.Posts.Include(p => p.Category).ToListAsync();
            var categories = await _context.Categories.ToListAsync();
            var settings = await _context.Settings.ToListAsync();

            var backup = new
            {
                Timestamp = DateTime.Now,
                Users = users,
                Posts = posts,
                Categories = categories,
                Settings = settings
            };

            var json = JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true });
            var fileName = $"backup_technews_{DateTime.Now:yyyyMMdd_HHmmss}.json";

            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
        }

        [HttpGet]
        [Route("api/system/logs")]
        public IActionResult GetLogs()
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (!Directory.Exists(logPath)) return Ok(new List<string>());

            var logFiles = Directory.GetFiles(logPath, "log-*.txt")
                                    .OrderByDescending(f => f)
                                    .Take(1)
                                    .ToList();

            if (!logFiles.Any()) return Ok(new List<string> { "No logs found." });

            var lines = new List<string>();
            try
            {
                using (var fs = new FileStream(logFiles[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line)) lines.Add(line);
                    }
                }
                
                return Ok(lines.TakeLast(100).Reverse().ToList());
            }
            catch (Exception ex)
            {
                return Ok(new List<string> { $"Error reading logs: {ex.Message}" });
            }
        }
    }
}