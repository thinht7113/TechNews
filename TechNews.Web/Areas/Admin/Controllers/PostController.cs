using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TechNews.Application.Interfaces;
using TechNews.Application.DTOs;

namespace TechNews.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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
            var allPosts = await _postService.GetAllPostsAsync();
            var totalCount = allPosts.Count();
            var pagedPosts = allPosts.Skip((page - 1) * pageSize).Take(pageSize);
            return Json(new { data = pagedPosts, totalCount, page, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) });
        }

        [HttpGet]
        [Route("api/post/get/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null) return NotFound();
            return Json(post);
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
                    await _postService.UpdatePostAsync(id, model);
                    return Ok(new { success = true });
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
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
                await _postService.RestoreRevisionAsync(revisionId);
                return Ok(new { success = true });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("api/post/delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _postService.DeletePostAsync(id);
                return Ok(new { success = true });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("api/post/gettrash")]
        public async Task<IActionResult> GetTrash()
        {
            var posts = await _postService.GetTrashAsync();
            return Json(posts);
        }

        [HttpPost]
        [Route("api/post/restore/{id}")]
        public async Task<IActionResult> Restore(int id)
        {
            var success = await _postService.RestorePostAsync(id);
            if (!success) return NotFound();
            return Ok(new { success = true });
        }

        [HttpPost]
        [Route("api/post/permanentdelete/{id}")]
        public async Task<IActionResult> PermanentDelete(int id)
        {
            try
            {
                await _postService.PermanentDeletePostAsync(id);
                return Ok(new { success = true });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
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