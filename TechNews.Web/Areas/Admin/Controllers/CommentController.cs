using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNews.Domain.Entities;
using TechNews.Infrastructure.Data;

namespace TechNews.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : Controller
    {
        private readonly TechNewsDbContext _context;

        public CommentController(TechNewsDbContext context)
        {
            _context = context;
        }

        [Route("/Admin/Comment")]
        public IActionResult Index() => View("~/Areas/Admin/Views/Shared/Spa.cshtml");

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20)
        {
            var query = _context.Comments
                .Include(c => c.Post)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedDate);

            var totalCount = await query.CountAsync();

            var comments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedDate,
                    c.IsApproved,
                    PostTitle = c.Post != null ? c.Post.Title : "Bài viết đã xóa",
                    PostId = c.PostId,
                    UserName = c.User != null ? (c.User.FullName ?? c.User.UserName) : "Người dùng đã xóa",
                    UserAvatar = c.User != null ? c.User.Avatar : null,
                    c.ParentId
                })
                .ToListAsync();

            return Ok(new { data = comments, totalCount, page, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) });
        }

        [HttpPost("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            comment.IsApproved = true;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost("reply")]
        public async Task<IActionResult> Reply([FromBody] ReplyRequest request)
        {
            var parent = await _context.Comments.FindAsync(request.ParentId);
            if (parent == null) return NotFound("Parent comment not found");

            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            var reply = new Comment
            {
                PostId = parent.PostId,
                UserId = adminUser?.Id ?? parent.UserId,
                Content = request.Content,
                ParentId = request.ParentId,
                IsApproved = true,
                CreatedDate = DateTime.Now
            };

            _context.Comments.Add(reply);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        public class ReplyRequest
        {
            public int ParentId { get; set; }
            public string Content { get; set; }
        }
    }
}