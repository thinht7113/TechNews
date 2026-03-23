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
        private readonly IArticleScraperService _scraperService;
        private readonly IWebHostEnvironment _env;

        public PostController(IPostService postService, IArticleScraperService scraperService, IWebHostEnvironment env)
        {
            _postService = postService;
            _scraperService = scraperService;
            _env = env;
        }

        public IActionResult Index() => View("Spa");
        [Route("Create")]
        public IActionResult CreateView() => View("Spa");
        [Route("Edit/{id?}")]
        public IActionResult EditView(int id) => View("Spa");

        [HttpPost]
        [Route("api/post/scrape-url")]
        public async Task<IActionResult> ScrapeUrl([FromBody] ScrapeUrlRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Url))
                return BadRequest(new { success = false, message = "Vui lòng nhập URL." });

            var result = await _scraperService.ScrapeAsync(request.Url);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.ErrorMessage });

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            httpClient.Timeout = TimeSpan.FromSeconds(15);

            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "images");
            Directory.CreateDirectory(uploadDir);
            var thumbDir = Path.Combine(_env.WebRootPath, "uploads", "thumbnails");
            Directory.CreateDirectory(thumbDir);

            // Download thumbnail
            var localThumbnail = result.ThumbnailUrl;
            if (!string.IsNullOrEmpty(result.ThumbnailUrl) && result.ThumbnailUrl.StartsWith("http"))
            {
                localThumbnail = await DownloadImageAsync(httpClient, result.ThumbnailUrl, thumbDir, "thumbnails");
            }

            // Download all content images and replace URLs
            var contentHtml = result.Content ?? "";
            var imgMatches = System.Text.RegularExpressions.Regex.Matches(contentHtml, @"<img[^>]+src=""([^""]+)""");
            foreach (System.Text.RegularExpressions.Match match in imgMatches)
            {
                var originalUrl = match.Groups[1].Value;
                if (originalUrl.StartsWith("http"))
                {
                    var localUrl = await DownloadImageAsync(httpClient, originalUrl, uploadDir, "images");
                    contentHtml = contentHtml.Replace(originalUrl, localUrl);
                }
            }

            return Ok(new
            {
                success = true,
                title = result.Title,
                shortDescription = result.ShortDescription,
                content = contentHtml,
                thumbnailUrl = localThumbnail,
                tags = result.Tags,
                sourceUrl = result.SourceUrl
            });
        }

        private async Task<string> DownloadImageAsync(HttpClient httpClient, string imageUrl, string saveDir, string folder)
        {
            try
            {
                var bytes = await httpClient.GetByteArrayAsync(imageUrl);
                var ext = Path.GetExtension(new Uri(imageUrl).AbsolutePath);
                if (string.IsNullOrEmpty(ext) || ext.Length > 5) ext = ".jpg";
                var fileName = $"{Guid.NewGuid()}_vne{ext}";
                await System.IO.File.WriteAllBytesAsync(Path.Combine(saveDir, fileName), bytes);
                return $"/uploads/{folder}/{fileName}";
            }
            catch
            {
                return imageUrl; // Keep original URL on failure
            }
        }


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

    public class ScrapeUrlRequest
    {
        public string Url { get; set; } = "";
    }
}