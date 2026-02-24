using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TechNews.Application.Interfaces;

namespace TechNews.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor")]
    public class WorkflowController : Controller
    {
        private readonly IWorkflowService _workflowService;

        public WorkflowController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        public IActionResult Index() => View("Spa");

        [HttpPost]
        [Route("api/workflow/submit/{postId}")]
        public async Task<IActionResult> Submit(int postId, [FromBody] WorkflowActionDto? dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _workflowService.SubmitForReviewAsync(postId, userId, dto?.Comment);
                return Ok(new { success = true, message = "Bài viết đã được gửi duyệt." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Không tìm thấy bài viết." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/workflow/approve/{postId}")]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<IActionResult> Approve(int postId, [FromBody] WorkflowActionDto? dto)
        {
            try
            {
                var editorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _workflowService.ApproveAsync(postId, editorId, dto?.Comment);
                return Ok(new { success = true, message = "Bài viết đã được duyệt và xuất bản." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Không tìm thấy bài viết." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/workflow/reject/{postId}")]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<IActionResult> Reject(int postId, [FromBody] WorkflowActionDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto?.Comment))
                    return BadRequest(new { message = "Lý do từ chối là bắt buộc." });

                var editorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _workflowService.RejectAsync(postId, editorId, dto.Comment);
                return Ok(new { success = true, message = "Bài viết đã bị từ chối." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Không tìm thấy bài viết." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/workflow/schedule/{postId}")]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<IActionResult> Schedule(int postId, [FromBody] ScheduleDto dto)
        {
            try
            {
                if (dto?.PublishDate == null)
                    return BadRequest(new { message = "Ngày xuất bản là bắt buộc." });

                var editorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _workflowService.ScheduleAsync(postId, editorId, dto.PublishDate, dto.Comment);
                return Ok(new { success = true, message = $"Bài viết sẽ được xuất bản lúc {dto.PublishDate:dd/MM/yyyy HH:mm}." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Không tìm thấy bài viết." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/workflow/pending")]
        public async Task<IActionResult> GetPending()
        {
            var posts = await _workflowService.GetPendingReviewsAsync();
            return Ok(posts);
        }

        [HttpGet]
        [Route("api/workflow/logs/{postId}")]
        public async Task<IActionResult> GetLogs(int postId)
        {
            var logs = await _workflowService.GetWorkflowLogsAsync(postId);
            return Ok(logs);
        }
    }

    public class WorkflowActionDto
    {
        public string? Comment { get; set; }
    }

    public class ScheduleDto
    {
        public DateTime PublishDate { get; set; }
        public string? Comment { get; set; }
    }
}
