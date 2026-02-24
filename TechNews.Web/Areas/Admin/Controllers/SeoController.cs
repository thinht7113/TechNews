using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechNews.Application.DTOs;
using TechNews.Application.Interfaces;

namespace TechNews.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor")]
    public class SeoController : Controller
    {
        private readonly ISeoService _seoService;

        public SeoController(ISeoService seoService)
        {
            _seoService = seoService;
        }

        [HttpPost]
        [Route("api/seo/analyze")]
        public IActionResult Analyze([FromBody] SeoAnalysisRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Title))
                return BadRequest(new { message = "Tiêu đề là bắt buộc." });

            var result = _seoService.Analyze(request);
            return Ok(result);
        }
    }
}
