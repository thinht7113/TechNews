using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechNews.Application.Interfaces;

namespace TechNews.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor")]
    public class AiController : Controller
    {
        private readonly IAiService _aiService;

        public AiController(IAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpGet]
        [Route("api/ai/status")]
        public IActionResult Status()
        {
            return Ok(new { configured = _aiService.IsConfigured });
        }

        [HttpPost]
        [Route("api/ai/generate")]
        public async Task<IActionResult> Generate([FromBody] AiPromptDto dto)
        {
            if (string.IsNullOrEmpty(dto?.Prompt))
                return BadRequest(new { message = "Prompt là bắt buộc." });

            try
            {
                var result = await _aiService.GenerateContentAsync(dto.Prompt, dto.Context);
                return Ok(new { success = true, content = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi AI: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("api/ai/summarize")]
        public async Task<IActionResult> Summarize([FromBody] AiContentDto dto)
        {
            if (string.IsNullOrEmpty(dto?.Content))
                return BadRequest(new { message = "Nội dung là bắt buộc." });

            try
            {
                var result = await _aiService.SummarizeAsync(dto.Content, dto.MaxLength > 0 ? dto.MaxLength : 200);
                return Ok(new { success = true, summary = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi AI: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("api/ai/suggest-tags")]
        public async Task<IActionResult> SuggestTags([FromBody] AiContentDto dto)
        {
            if (string.IsNullOrEmpty(dto?.Content))
                return BadRequest(new { message = "Nội dung là bắt buộc." });

            try
            {
                var tags = await _aiService.SuggestTagsAsync(dto.Content);
                return Ok(new { success = true, tags });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi AI: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("api/ai/improve")]
        public async Task<IActionResult> Improve([FromBody] AiContentDto dto)
        {
            if (string.IsNullOrEmpty(dto?.Content))
                return BadRequest(new { message = "Nội dung là bắt buộc." });

            try
            {
                var result = await _aiService.ImproveWritingAsync(dto.Content);
                return Ok(new { success = true, content = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi AI: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("api/ai/suggest-titles")]
        public async Task<IActionResult> SuggestTitles([FromBody] AiContentDto dto)
        {
            if (string.IsNullOrEmpty(dto?.Content))
                return BadRequest(new { message = "Nội dung là bắt buộc." });

            try
            {
                var titles = await _aiService.GenerateTitlesAsync(dto.Content);
                return Ok(new { success = true, titles });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi AI: {ex.Message}" });
            }
        }
    }

    public class AiPromptDto
    {
        public string Prompt { get; set; } = string.Empty;
        public string? Context { get; set; }
    }

    public class AiContentDto
    {
        public string Content { get; set; } = string.Empty;
        public int MaxLength { get; set; } = 200;
    }
}
