using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechNews.Application.Interfaces;

namespace TechNews.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        public IActionResult Index() => View("Spa");

        /// <summary>
        /// Public endpoint — frontend JS sends tracking data here
        /// </summary>
        [HttpPost]
        [Route("api/analytics/track")]
        [AllowAnonymous]
        public async Task<IActionResult> Track([FromBody] TrackPageViewDto dto)
        {
            if (dto == null || dto.PostId <= 0)
                return BadRequest();

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
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
