using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;
using TechNews.Application.Interfaces;

namespace TechNews.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analyticsService;

        // Bug 7 fix: Simple IP-based rate limiting
        private static readonly ConcurrentDictionary<string, (int count, DateTime resetAt)> _rateLimits = new();
        private const int MaxRequestsPerMinute = 10;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        public IActionResult Index() => View("Spa");

        /// <summary>
        /// Public endpoint — frontend JS sends tracking data here
        /// Bug 7 fix: Rate limited to 10 requests/minute per IP
        /// </summary>
        [HttpPost]
        [Route("api/analytics/track")]
        [AllowAnonymous]
        public async Task<IActionResult> Track([FromBody] TrackPageViewDto dto)
        {
            if (dto == null || dto.PostId <= 0)
                return BadRequest();

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Rate limiting check
            var now = DateTime.Now;
            var entry = _rateLimits.AddOrUpdate(ip,
                _ => (1, now.AddMinutes(1)),
                (_, existing) =>
                {
                    if (now >= existing.resetAt)
                        return (1, now.AddMinutes(1));
                    return (existing.count + 1, existing.resetAt);
                });

            if (entry.count > MaxRequestsPerMinute)
                return StatusCode(429, new { message = "Too many requests" });

            var userAgent = Request.Headers.UserAgent.ToString();

            await _analyticsService.TrackPageViewAsync(
                dto.PostId,
                dto.SessionId,
                ip,
                userAgent,
                dto.Referrer,
                dto.TimeOnPage,
                dto.ScrollDepth
            );

            return Ok(new { success = true });
        }

        [HttpGet]
        [Route("api/analytics/overview")]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<IActionResult> Overview(int days = 30)
        {
            var data = await _analyticsService.GetOverviewAnalyticsAsync(days);
            return Ok(data);
        }

        [HttpGet]
        [Route("api/analytics/post/{id}")]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<IActionResult> PostAnalytics(int id, int days = 30)
        {
            var data = await _analyticsService.GetPostAnalyticsAsync(id, days);
            return Ok(data);
        }
    }

    public class TrackPageViewDto
    {
        public int PostId { get; set; }
        public string? SessionId { get; set; }
        public string? Referrer { get; set; }
        public int TimeOnPage { get; set; }
        public int ScrollDepth { get; set; }
    }
}
