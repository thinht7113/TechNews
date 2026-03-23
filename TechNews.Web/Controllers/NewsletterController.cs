using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TechNews.Domain.Entities;
using TechNews.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace TechNews.Web.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly IRepository<Subscriber> _subscriberRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public NewsletterController(IRepository<Subscriber> subscriberRepo, IUnitOfWork unitOfWork, IMemoryCache cache)
        {
            _subscriberRepo = subscriberRepo;
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        private bool IsRateLimited(string action)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var cacheKey = $"Newsletter_{action}_{ip}";
            var attempts = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return 0;
            });

            if (attempts >= 5) return true; // Max 5 subscribe attempts per 10 min

            _cache.Set(cacheKey, attempts + 1, TimeSpan.FromMinutes(10));
            return false;
        }

        [HttpPost]
        [Route("api/newsletter/subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest model)
        {
            if (IsRateLimited("subscribe"))
                return StatusCode(429, new { message = "Bạn đã thử quá nhiều lần. Vui lòng đợi." });

            if (!ModelState.IsValid)
                return BadRequest(new { message = "Email không hợp lệ" });

            var existing = (await _subscriberRepo.FindAsync(s => s.Email == model.Email)).FirstOrDefault();
            if (existing != null)
            {
                if (existing.IsActive)
                    return Ok(new { success = true, message = "Email này đã đăng ký nhận tin rồi!" });

                existing.IsActive = true;
                existing.UnsubscribedDate = null;
                await _subscriberRepo.UpdateAsync(existing);
                await _unitOfWork.CompleteAsync();
                return Ok(new { success = true, message = "Chào mừng bạn quay lại! Đã kích hoạt lại đăng ký." });
            }

            var subscriber = new Subscriber
            {
                Email = model.Email,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _subscriberRepo.AddAsync(subscriber);
            await _unitOfWork.CompleteAsync();

            return Ok(new { success = true, message = "Đăng ký nhận tin thành công! Cảm ơn bạn." });
        }

        [HttpPost]
        [Route("api/newsletter/unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromBody] SubscribeRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Email không hợp lệ" });

            var subscriber = (await _subscriberRepo.FindAsync(s => s.Email == model.Email && s.IsActive)).FirstOrDefault();
            if (subscriber == null)
                return NotFound(new { message = "Email chưa đăng ký nhận tin" });

            subscriber.IsActive = false;
            subscriber.UnsubscribedDate = DateTime.UtcNow;
            await _subscriberRepo.UpdateAsync(subscriber);
            await _unitOfWork.CompleteAsync();

            return Ok(new { success = true, message = "Đã hủy đăng ký nhận tin." });
        }

        public class SubscribeRequest
        {
            [Required(ErrorMessage = "Email không được để trống")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = string.Empty;
        }
    }
}