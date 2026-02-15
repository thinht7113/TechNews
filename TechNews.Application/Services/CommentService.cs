using TechNews.Application.Interfaces;
using TechNews.Domain.Entities;
using TechNews.Domain.Interfaces;

namespace TechNews.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly IRepository<Comment> _commentRepo;
        private readonly IUnitOfWork _unitOfWork;

        public CommentService(IRepository<Comment> commentRepo, IUnitOfWork unitOfWork)
        {
            _commentRepo = commentRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<(IEnumerable<object> Data, int TotalCount)> GetAllCommentsAsync(int page = 1, int pageSize = 20)
        {
            var all = (await _commentRepo.GetAllAsync(c => c.Post, c => c.User))
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            var totalCount = all.Count;
            var data = all
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedDate,
                    c.IsApproved,
                    PostTitle = c.Post != null ? c.Post.Title : "Bài viết đã xóa",
                    PostId = c.PostId,
                    UserName = c.User != null ? (c.User.FullName ?? c.User.UserName) : "Người dùng đã xóa",
                    UserAvatar = c.User != null ? c.User.Avatar : null,
                    c.ParentId
                });

            return (data, totalCount);
        }

        public async Task ApproveCommentAsync(int id)
        {
            var comments = await _commentRepo.FindAsync(c => c.Id == id);
            var comment = comments.FirstOrDefault();
            if (comment == null) throw new KeyNotFoundException("Không tìm thấy bình luận");

            comment.IsApproved = true;
            await _commentRepo.UpdateAsync(comment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteCommentAsync(int id)
        {
            var comments = await _commentRepo.FindAsync(c => c.Id == id);
            var comment = comments.FirstOrDefault();
            if (comment == null) throw new KeyNotFoundException("Không tìm thấy bình luận");

            await _commentRepo.DeleteAsync(comment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task ReplyToCommentAsync(int parentId, string content, string userId)
        {
            var parents = await _commentRepo.FindAsync(c => c.Id == parentId);
            var parent = parents.FirstOrDefault();
            if (parent == null) throw new KeyNotFoundException("Không tìm thấy bình luận gốc");

            var reply = new Comment
            {
                PostId = parent.PostId,
                UserId = userId,
                Content = content,
                ParentId = parentId,
                IsApproved = true,
                CreatedDate = DateTime.Now
            };

            await _commentRepo.AddAsync(reply);
            await _unitOfWork.CompleteAsync();
        }
    }
}
