using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNews.Domain.Entities;
using TechNews.Domain.Interfaces;

namespace Project_News.Controllers
{
    public class PostController : Controller
    {
        private readonly IRepository<Post> _postRepo;
        private readonly IRepository<Comment> _commentRepo;
        private readonly IUnitOfWork _unitOfWork;

        public PostController(IRepository<Post> postRepo, IRepository<Comment> commentRepo, IUnitOfWork unitOfWork)
        {
            _postRepo = postRepo;
            _commentRepo = commentRepo;
            _unitOfWork = unitOfWork;
        }

        [Route("post/{slug}")]
        public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return NotFound();

            var posts = await _postRepo.GetAllAsync(p => p.Category, p => p.Author);
            var post = posts.FirstOrDefault(p => p.Slug == slug && !p.IsDeleted && p.Status == TechNews.Domain.Enums.PostStatus.Published);

            if (post == null) return NotFound();

            post.ViewCount++;
            await _postRepo.UpdateAsync(post);
            await _unitOfWork.CompleteAsync();

            var comments = await _commentRepo.FindAsync(c => c.PostId == post.Id && c.IsApproved, c => c.User);
            post.Comments = comments.OrderByDescending(c => c.CreatedDate).ToList();

            var relatedPosts = posts
                .Where(p => p.CategoryId == post.CategoryId && p.Id != post.Id && !p.IsDeleted && p.Status == TechNews.Domain.Enums.PostStatus.Published)
                .OrderByDescending(p => p.CreatedDate)
                .Take(3)
                .ToList();

            ViewBag.RelatedPosts = relatedPosts;

            return View(post);
        }

        [Route("post/search")]
        public async Task<IActionResult> Search(string query, int page = 1)
        {
            var posts = await _postRepo.GetAllAsync(p => p.Category, p => p.Author);
            
            if (string.IsNullOrWhiteSpace(query))
            {
                 return View(new List<Post>());
            }

            query = query.ToLower();
            var allResults = posts
                .Where(p => !p.IsDeleted && p.Status == TechNews.Domain.Enums.PostStatus.Published &&
                            (p.Title.ToLower().Contains(query) || 
                            (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(query))))
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