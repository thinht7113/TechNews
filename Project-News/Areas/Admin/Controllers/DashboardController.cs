using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechNews.Domain.Entities;
using TechNews.Domain.Interfaces;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using TechNews.Domain.Enums;

namespace Project_News.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IRepository<Post> _postRepo;
        private readonly IRepository<Category> _catRepo;
        private readonly UserManager<User> _userManager;

        public DashboardController(IRepository<Post> postRepo, IRepository<Category> catRepo, UserManager<User> userManager)
        {
            _postRepo = postRepo;
            _catRepo = catRepo;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View("Spa");
        }

        [HttpGet]
        [Route("api/dashboard/stats")]
        public async Task<IActionResult> GetStats()
        {
            var posts = await _postRepo.GetAllAsync();
            var cats = await _catRepo.GetAllAsync();
            var users = await _userManager.Users.ToListAsync();
            
            var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-i)).Reverse().ToList();
            var chartLabels = last7Days.Select(d => d.ToString("dd/MM")).ToList();
            var chartData = last7Days.Select(d => posts.Count(p => p.CreatedDate.Date == d)).ToList();

            var stats = new
            {
                TotalPosts = posts.Count(),
                TotalCategories = cats.Count(),
                DraftPosts = posts.Count(p => p.Status != PostStatus.Published),
                MediaCount = System.IO.Directory.GetFiles(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads"), "*", System.IO.SearchOption.AllDirectories).Length,
                RecentActivity = posts.OrderByDescending(p => p.CreatedDate).Take(5).Select(p => new { p.Title, Date = p.CreatedDate.ToString("dd/MM") }),
                
                Chart = new { Labels = chartLabels, Data = chartData },
                TopPosts = posts.OrderByDescending(p => p.ViewCount).Take(5).Select(p => new { p.Title, p.ViewCount, p.Slug }),
                RecentUsers = users.OrderByDescending(u => u.Id).Take(5).Select(u => new { u.Email, u.FullName, Joined = "Mới đây" })
            };

            return Json(stats);
        }
    }
}