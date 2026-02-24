using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechNews.Domain.Entities;
using TechNews.Domain.Enums;
using TechNews.Domain.Interfaces;

namespace TechNews.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor")]
    public class CalendarController : Controller
    {
        private readonly IRepository<Post> _postRepo;

        public CalendarController(IRepository<Post> postRepo)
        {
            _postRepo = postRepo;
        }

        public IActionResult Index() => View("Spa");

        /// <summary>
        /// Returns events for FullCalendar.js
        /// </summary>
        [HttpGet]
        [Route("api/calendar/events")]
        public async Task<IActionResult> GetEvents(DateTime? start, DateTime? end)
        {
            var from = start ?? DateTime.Now.AddMonths(-1);
            var to = end ?? DateTime.Now.AddMonths(2);

            var posts = await _postRepo.GetAllAsync(p => p.Category, p => p.Author);
            var events = posts
                .Where(p => !p.IsDeleted && (
                    p.Status == PostStatus.Published ||
                    p.Status == PostStatus.Scheduled ||
                    p.Status == PostStatus.InReview ||
                    p.Status == PostStatus.Draft
                ))
                .Select(p => new
                {
                    id = p.Id,
                    title = p.Title,
                    start = p.Status == PostStatus.Scheduled && p.ScheduledPublishDate.HasValue
                        ? p.ScheduledPublishDate.Value.ToString("yyyy-MM-ddTHH:mm:ss")
                        : p.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = GetStatusColor(p.Status),
                    status = p.Status.ToString(),
                    category = p.Category?.Name,
                    author = p.Author?.FullName ?? p.Author?.Email,
                    slug = p.Slug
                })
                .ToList();

            return Ok(events);
        }

        [HttpPost]
        [Route("api/calendar/schedule")]
        public async Task<IActionResult> Schedule([FromBody] CalendarScheduleDto dto)
        {
            if (dto == null || dto.PostId <= 0)
                return BadRequest(new { message = "PostId là bắt buộc." });

            if (dto.PublishDate <= DateTime.Now)
                return BadRequest(new { message = "Ngày xuất bản phải trong tương lai." });

            var post = await _postRepo.GetByIdAsync(dto.PostId);
            if (post == null) return NotFound();

            post.Status = PostStatus.Scheduled;
            post.ScheduledPublishDate = dto.PublishDate;
            post.ModifiedDate = DateTime.Now;
            await _postRepo.UpdateAsync(post);

            return Ok(new { success = true, message = $"Đã lên lịch xuất bản lúc {dto.PublishDate:dd/MM/yyyy HH:mm}." });
        }

        private static string GetStatusColor(PostStatus status) => status switch
        {
            PostStatus.Published => "#10b981",  // emerald
            PostStatus.Scheduled => "#3b82f6",  // blue
            PostStatus.InReview => "#f59e0b",   // amber
            PostStatus.Draft => "#6b7280",      // gray
            PostStatus.Rejected => "#ef4444",   // red
            _ => "#6b7280"
        };
    }

    public class CalendarScheduleDto
    {
        public int PostId { get; set; }
        public DateTime PublishDate { get; set; }
    }
}
