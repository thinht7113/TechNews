using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Project_News.Models;
using TechNews.Domain.Entities;
using TechNews.Domain.Interfaces;

using TechNews.Domain.Enums;

namespace Project_News.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepository<Post> _postRepo;
        private readonly IRepository<Contact> _contactRepo;
        private readonly IRepository<Tag> _tagRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public HomeController(ILogger<HomeController> logger, IRepository<Post> postRepo, IRepository<Contact> contactRepo, IRepository<Tag> tagRepo, IUnitOfWork unitOfWork, IMemoryCache cache)
        {
            _logger = logger;
            _postRepo = postRepo;
            _contactRepo = contactRepo;
            _tagRepo = tagRepo;
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            const string cacheKey = "HomeIndex_ViewModel";
            var viewModel = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                var allPosts = (await _postRepo.GetAllAsync(p => p.Category, p => p.Author))
                    .Where(p => p.Status == PostStatus.Published && !p.IsDeleted)
                    .OrderByDescending(p => p.CreatedDate)
                    .ToList();
                
                var featuredPosts = allPosts.Take(5).ToList();
                var featuredMain = featuredPosts.FirstOrDefault();
                var featuredSub = featuredPosts.Skip(1).ToList();
                var latestStream = allPosts.Skip(5).Take(10).ToList();
                
                var categorySections = allPosts
                    .Where(p => p.Category != null)
                    .GroupBy(p => p.Category)
                    .Select(g => new CategorySection
                    {
                        CategoryId = g.Key.Id,
                        CategoryName = g.Key.Name,
                        CategorySlug = g.Key.Slug,
                        Posts = g.OrderByDescending(p => p.CreatedDate).Take(4).ToList()
                    })
                    .OrderBy(c => c.CategoryName)
                    .ToList();

                var mostViewed = allPosts.OrderByDescending(p => p.ViewCount).Take(5).ToList();

                var trendingTags = (await _tagRepo.GetAllAsync())
                    .OrderByDescending(t => t.Count)
                    .Take(10)
                    .Select(t => t.Name)
                    .ToList();

                return new HomeIndexViewModel
                {
                    FeaturedMain = featuredMain,
                    FeaturedSub = featuredSub,
                    LatestStream = latestStream,
                    MostViewed = mostViewed,
                    CategorySections = categorySections,
                    TrendingTags = trendingTags
                };
            });

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Tag(string slug, int page = 1)
        {
             if (string.IsNullOrEmpty(slug)) return RedirectToAction(nameof(Index));

             var tag = (await _tagRepo.FindAsync(t => t.Slug == slug)).FirstOrDefault();
             if (tag == null) return NotFound();

             int pageSize = 12;
             var posts = (await _postRepo.FindAsync(p => p.PostTags.Any(pt => pt.TagId == tag.Id), p => p.Category, p => p.Author))
                        .OrderByDescending(p => p.CreatedDate)
                        .ToList();
             
             var pagedPosts = posts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

             ViewBag.TagName = tag.Name;
             ViewBag.TagSlug = tag.Slug;
             ViewBag.CurrentPage = page;
             ViewBag.TotalPages = (int)Math.Ceiling(posts.Count / (double)pageSize);

             return View(pagedPosts);
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                var contact = new Contact
                {
                    Name = model.Name,
                    Email = model.Email,
                    Subject = model.Subject,
                    Message = model.Message,
                    CreatedDate = DateTime.Now,
                    IsRead = false
                };

                await _contactRepo.AddAsync(contact);
                await _unitOfWork.CompleteAsync();
                
                TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất.";
                return RedirectToAction(nameof(Contact));
            }
            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> LoadMore(int page = 1)
        {
            int pageSize = 10;
            int skip = 15 + ((page - 1) * pageSize);
            
            var posts = (await _postRepo.GetAllAsync(p => p.Category, p => p.Author))
                .OrderByDescending(p => p.CreatedDate)
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            if (!posts.Any()) return NoContent();

            return PartialView("_PostListPartial", posts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("/Home/Error404")]
        public IActionResult Error404() => View();

        [Route("/Home/Error403")]
        public IActionResult Error403() => View();

        [Route("/Home/Error500")]
        public IActionResult Error500() => View();
    }
}

