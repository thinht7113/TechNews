using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechNews.Application.Interfaces;
using TechNews.Domain.Entities;
using TechNews.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Project_News.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NewsletterController : Controller
    {
        private readonly IRepository<Subscriber> _subscriberRepo;
        private readonly IRepository<Post> _postRepo;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;

        public NewsletterController(
            IRepository<Subscriber> subscriberRepo,
            IRepository<Post> postRepo,
            IEmailService emailService,
            IUnitOfWork unitOfWork)
        {
            _subscriberRepo = subscriberRepo;
            _postRepo = postRepo;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index() => View("~/Areas/Admin/Views/Shared/Spa.cshtml");


        [HttpGet]
        [Route("api/newsletter/subscribers")]
        public async Task<IActionResult> GetSubscribers(int page = 1, int pageSize = 20)
        {
            var all = (await _subscriberRepo.GetAllAsync())
                .OrderByDescending(s => s.CreatedDate)
                .ToList();

            var totalCount = all.Count;
            var activeCount = all.Count(s => s.IsActive);
            var data = all
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    s.Id,
                    s.Email,
                    s.IsActive,
                    s.CreatedDate,
                    s.UnsubscribedDate
                });

            return Ok(new { data, totalCount, activeCount, page, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) });
        }

        [HttpPost]
        [Route("api/newsletter/subscribers/delete/{id}")]
        public async Task<IActionResult> DeleteSubscriber(int id)
        {
            var subs = await _subscriberRepo.FindAsync(s => s.Id == id);
            var sub = subs.FirstOrDefault();
            if (sub == null) return NotFound();

            await _subscriberRepo.DeleteAsync(sub);
            await _unitOfWork.CompleteAsync();
            return Ok(new { success = true });
        }

        [HttpGet]
        [Route("api/newsletter/posts")]
        public async Task<IActionResult> GetPublishedPosts()
        {
            var posts = (await _postRepo.GetAllAsync())
                .Where(p => p.Status == TechNews.Domain.Enums.PostStatus.Published && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedDate)
                .Take(50)
                .Select(p => new { p.Id, p.Title, p.Slug, p.CreatedDate });

            return Ok(posts);
        }

        [HttpPost]
        [Route("api/newsletter/send")]
        public async Task<IActionResult> SendNewsletter([FromBody] SendNewsletterRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });

            var subscribers = (await _subscriberRepo.FindAsync(s => s.IsActive))
                .Select(s => s.Email)
                .ToList();

            if (!subscribers.Any())
                return BadRequest(new { message = "Chưa có người đăng ký nhận tin nào" });

            var htmlBody = BuildNewsletterHtml(model.Subject, model.Content, model.PostIds);

            try
            {
                await _emailService.SendBulkEmailAsync(subscribers, model.Subject, htmlBody);
                return Ok(new { success = true, message = $"Đã gửi newsletter đến {subscribers.Count} người đăng ký" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi gửi email: {ex.Message}" });
            }
        }

        private string BuildNewsletterHtml(string subject, string content, List<int>? postIds)
        {
            var postsHtml = "";
            if (postIds != null && postIds.Any())
            {
                var posts = _postRepo.FindAsync(p => postIds.Contains(p.Id)).Result;
                postsHtml = string.Join("", posts.Select(p => $@"
                    <tr>
                        <td style='padding: 12px 0; border-bottom: 1px solid #eee;'>
                            <a href='https://localhost/post/{p.Slug}' style='color: #3C50E0; text-decoration: none; font-weight: 600; font-size: 16px;'>{p.Title}</a>
                            <p style='margin: 4px 0 0; color: #666; font-size: 13px;'>{p.CreatedDate:dd/MM/yyyy}</p>
                        </td>
                    </tr>
                "));
            }

            return $@"
                <!DOCTYPE html>
                <html>
                <head><meta charset='utf-8'></head>
                <body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; margin: 0; padding: 0; background: #f4f4f7;'>
                    <table width='100%' cellpadding='0' cellspacing='0' style='max-width: 600px; margin: 0 auto; background: #ffffff;'>
                        <tr>
                            <td style='background: linear-gradient(135deg, #3C50E0, #6366f1); padding: 30px; text-align: center;'>
                                <h1 style='color: white; margin: 0; font-size: 24px;'>📰 TechNews</h1>
                            </td>
                        </tr>
                        <tr>
                            <td style='padding: 30px;'>
                                <h2 style='color: #1a1a2e; margin: 0 0 16px;'>{subject}</h2>
                                <div style='color: #444; line-height: 1.6; font-size: 15px;'>{content}</div>
                                {(string.IsNullOrEmpty(postsHtml) ? "" : $@"
                                    <h3 style='color: #1a1a2e; margin: 24px 0 12px; font-size: 18px;'>📌 Bài viết nổi bật</h3>
                                    <table width='100%' cellpadding='0' cellspacing='0'>{postsHtml}</table>
                                ")}
                            </td>
                        </tr>
                        <tr>
                            <td style='background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #888;'>
                                <p>Bạn nhận email này vì đã đăng ký nhận tin từ TechNews.</p>
                                <a href='https://localhost/api/newsletter/unsubscribe' style='color: #3C50E0;'>Hủy đăng ký</a>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>
            ";
        }

        public class SendNewsletterRequest
        {
            [Required(ErrorMessage = "Tiêu đề không được để trống")]
            [StringLength(200)]
            public string Subject { get; set; } = string.Empty;

            [Required(ErrorMessage = "Nội dung không được để trống")]
            public string Content { get; set; } = string.Empty;

            public List<int>? PostIds { get; set; }
        }
    }
}