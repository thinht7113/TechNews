using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechNews.Domain.Entities;
using TechNews.Domain.Interfaces;

namespace Project_News.Controllers
{
    public class CommentController : Controller
    {
        private readonly IRepository<Comment> _commentRepo;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public CommentController(IRepository<Comment> commentRepo, UserManager<User> userManager, IUnitOfWork unitOfWork)
        {
            _commentRepo = commentRepo;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int postId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Nội dung không được để trống");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var comment = new Comment
            {
                PostId = postId,
                UserId = user.Id,
                Content = content,
                CreatedDate = DateTime.Now,
                IsApproved = true
            };

            await _commentRepo.AddAsync(comment);
            await _unitOfWork.CompleteAsync();
            
            comment.User = user;

            return PartialView("_CommentItemPartial", comment);
        }
    }
}