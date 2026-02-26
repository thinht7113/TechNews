using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TechNews.Application.Interfaces;
using TechNews.Application.DTOs;

namespace TechNews.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor")]
    public class PostController : Controller
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        public IActionResult Index() => View("Spa");
        [Route("Create")]
        public IActionResult CreateView() => View("Spa");
        [Route("Edit/{id?}")]
        public IActionResult EditView(int id) => View("Spa");


        [HttpGet]
        [Route("api/post/getall")]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var allPosts = await _postService.GetAllPostsAsync(userId, isAdmin);
            var totalCount = allPosts.Count();
            var pagedPosts = allPosts.Skip((page - 1) * pageSize).Take(pageSize);
            return Json(new { data = pagedPosts, totalCount, page, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) });
        }

        [HttpGet]
        [Route("api/post/get/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                var post = await _postService.GetPostByIdAsync(id, userId, isAdmin);
                if (post == null) return NotFound();
                return Json(post);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/post/create")]
        public async Task<IActionResult> Create([FromForm] CreatePostDto model)
        {
            if (ModelState.IsValid)
            {
                var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _postService.CreatePostAsync(model, authorId!);
                return Ok(new { success = true });
            }
            return BadRequest(new { message = "Dữ liệu không hợp lệ" });
        }

        [HttpPost]
        [Route("api/post/update/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] EditPostDto model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var isAdmin = User.IsInRole("Admin");

                    await _postService.UpdatePostAsync(id, model, userId, isAdmin);
                    return Ok(new { success = true });
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (UnauthorizedAccessException ex)
                {
                    return StatusCode(403, new { message = ex.Message });
                }
            }
            return BadRequest(new { message = "Dữ liệu không hợp lệ" });
        }

        [HttpGet]
        [Route("api/post/revisions/{id}")]
        public async Task<IActionResult> GetRevisions(int id)
        {
            var revisions = await _postService.GetRevisionsAsync(id);
            return Ok(revisions);
        }

        [HttpPost]
        [Route("api/post/restore/{revisionId}")]
        public async Task<IActionResult> RestoreRevision(int revisionId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                await _postService.RestoreRevisionAsync(revisionId, userId, isAdmin);
                return Ok(new { success = true });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/post/delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                await _postService.DeletePostAsync(id, userId, isAdmin);
                return Ok(new { success = true });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/post/gettrash")]
        public async Task<IActionResult> GetTrash()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var posts = await _postService.GetTrashAsync(userId, isAdmin);
            return Json(posts);
        }

        [HttpPost]
        [Route("api/post/restore/{id}")]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                var success = await _postService.RestorePostAsync(id, userId, isAdmin);
                if (!success) return NotFound();
                return Ok(new { success = true });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/post/permanentdelete/{id}")]
        public async Task<IActionResult> PermanentDelete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                await _postService.PermanentDeletePostAsync(id, userId, isAdmin);
                return Ok(new { success = true });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/post/uploadimage")]
        public async Task<IActionResult> UploadImage(IFormFile upload)
        {
             var url = await _postService.UploadImageAsync(upload);
             if (!string.IsNullOrEmpty(url))
             {
                 return Json(new { url });
             }
             return Json(new { error = new { message = "Upload failed" } });
        }
    }
}