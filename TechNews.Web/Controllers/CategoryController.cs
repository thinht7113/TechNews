using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNews.Domain.Entities;
using TechNews.Domain.Interfaces;

namespace TechNews.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IRepository<Category> _categoryRepo;
        private readonly IRepository<Post> _postRepo;

        public CategoryController(IRepository<Category> categoryRepo, IRepository<Post> postRepo)
        {
            _categoryRepo = categoryRepo;
            _postRepo = postRepo;
        }

        [Route("category/{slug}")]
        public async Task<IActionResult> Index(string slug, int page = 1)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var categories = await _categoryRepo.FindAsync(c => c.Slug == slug);
            var category = categories.FirstOrDefault();

            if (category == null)
            {
                return NotFound();
            }

            var allPosts = (await _postRepo.FindAsync(p => p.CategoryId == category.Id && !p.IsDeleted && p.Status == TechNews.Domain.Enums.PostStatus.Published, p => p.Author))
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            int pageSize = 9; // Grid 3x3
            var pagedPosts = allPosts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CategoryName = category.Name;
            ViewBag.CategorySlug = category.Slug;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(allPosts.Count / (double)pageSize);

            return View(pagedPosts);
        }
    }
}
