using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TechNews.Application.Interfaces;
using TechNews.Domain.Entities;

namespace TechNews.Web.Controllers
{
    [Authorize]
    [Route("Profile")]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly UserManager<User> _userManager;

        public ProfileController(IProfileService profileService, UserManager<User> userManager)
        {
            _profileService = profileService;
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            
            return View(user);
        }

        [HttpGet("Comments")]
        public async Task<IActionResult> Comments([FromQuery] int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            int pageSize = 10;
            var comments = await _profileService.GetUserCommentsAsync(user.Id, pageSize, (page - 1) * pageSize);
            ViewBag.TotalCount = await _profileService.GetUserCommentsCountAsync(user.Id);
            ViewBag.CurrentPage = page;

            return View(comments);
        }

        [HttpGet("Saved")]
        public async Task<IActionResult> Saved([FromQuery] int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            int pageSize = 12;
            var savedPosts = await _profileService.GetSavedPostsAsync(user.Id, pageSize, (page - 1) * pageSize);
            ViewBag.TotalCount = await _profileService.GetSavedPostsCountAsync(user.Id);
            ViewBag.CurrentPage = page;

            return View(savedPosts);
        }

        [HttpGet("History")]
        public async Task<IActionResult> History([FromQuery] int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            int pageSize = 12;
            var historyPosts = await _profileService.GetViewHistoryAsync(user.Id, pageSize, (page - 1) * pageSize);
            ViewBag.TotalCount = await _profileService.GetViewHistoryCountAsync(user.Id);
            ViewBag.CurrentPage = page;

            return View(historyPosts);
        }

        [HttpPost("ToggleSavePost/{postId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSavePost(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { message = "You must be logged in to save posts." });

            var isSaved = await _profileService.ToggleSavePostAsync(user.Id, postId);
            
            return Ok(new { isSaved = isSaved, success = true });
        }
        
        [HttpGet("CheckSaved/{postId}")]
        public async Task<IActionResult> CheckSaved(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Ok(new { isSaved = false });

            var isSaved = await _profileService.IsPostSavedAsync(user.Id, postId);
            return Ok(new { isSaved = isSaved });
        }
        
    }
}
