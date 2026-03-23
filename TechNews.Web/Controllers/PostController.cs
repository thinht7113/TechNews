using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNews.Domain.Entities;
using TechNews.Domain.Interfaces;
using TechNews.Domain.Enums;
using TechNews.Infrastructure.Data;

namespace TechNews.Web.Controllers
{
    public class PostController : Controller
    {
        private readonly IRepository<Post> _postRepo;
        private readonly IRepository<Comment> _commentRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TechNews.Application.Interfaces.IProfileService _profileService;
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> _userManager;
        private readonly TechNewsDbContext _dbContext;

        public PostController(
            IRepository<Post> postRepo, 
            IRepository<Comment> commentRepo, 
            IUnitOfWork unitOfWork,
            TechNews.Application.Interfaces.IProfileService profileService,
            Microsoft.AspNetCore.Identity.UserManager<User> userManager,
            TechNewsDbContext dbContext)
        {
            _postRepo = postRepo;
            _commentRepo = commentRepo;
            _unitOfWork = unitOfWork;
            _profileService = profileService;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        [Route("post/{slug}")]
        public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return NotFound();

            var posts = await _postRepo.FindAsync(
                p => p.Slug == slug && !p.IsDeleted && p.Status == PostStatus.Published,
                p => p.Category, p => p.Author);
            var post = posts.FirstOrDefault();

            if (post == null) return NotFound();

            await _dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE Posts SET ViewCount = ViewCount + 1 WHERE Id = {0}", post.Id);
            post.ViewCount++; // Update the in-memory object for display

            var comments = await _commentRepo.FindAsync(c => c.PostId == post.Id && c.IsApproved, c => c.User);
            post.Comments = comments.OrderByDescending(c => c.CreatedDate).ToList();

            // Related posts: query only same category, exclude current post
            var relatedPosts = (await _postRepo.FindAsync(
                p => p.CategoryId == post.CategoryId && p.Id != post.Id && !p.IsDeleted && p.Status == PostStatus.Published,
                p => p.Category, p => p.Author))
                .OrderByDescending(p => p.CreatedDate)
                .Take(3)
                .ToList();

            ViewBag.RelatedPosts = relatedPosts;
            ViewBag.IsSaved = false;

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                await _profileService.RecordPostViewAsync(user.Id, post.Id);
                ViewBag.IsSaved = await _profileService.IsPostSavedAsync(user.Id, post.Id);
            }

            return View(post);
        }

        [Route("post/search")]
        public async Task<IActionResult> Search(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                 return View(new List<Post>());
            }

            var lowerQuery = query.ToLower();
            var allResults = (await _postRepo.FindAsync(
                p => !p.IsDeleted && p.Status == PostStatus.Published &&
                    (p.Title.ToLower().Contains(lowerQuery) || 
                    (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(lowerQuery))),
                p => p.Category, p => p.Author))
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            int pageSize = 9;
            var pagedResults = allResults.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Query = query;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(allResults.Count / (double)pageSize);
            
            return View(pagedResults);
        }
    }
}