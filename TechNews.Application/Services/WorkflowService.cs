using TechNews.Application.Interfaces;
using TechNews.Domain.Entities;
using TechNews.Domain.Enums;
using TechNews.Domain.Interfaces;

namespace TechNews.Application.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IRepository<Post> _postRepo;
        private readonly IRepository<WorkflowLog> _workflowLogRepo;
        private readonly IUnitOfWork _unitOfWork;

        public WorkflowService(
            IRepository<Post> postRepo,
            IRepository<WorkflowLog> workflowLogRepo,
            IUnitOfWork unitOfWork)
        {
            _postRepo = postRepo;
            _workflowLogRepo = workflowLogRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task SubmitForReviewAsync(int postId, string userId, string? comment = null)
        {
            var post = await _postRepo.GetByIdAsync(postId)
                ?? throw new KeyNotFoundException($"Post {postId} not found");

            if (post.Status != PostStatus.Draft && post.Status != PostStatus.Rejected)
                throw new InvalidOperationException("Only Draft or Rejected posts can be submitted for review.");

            var fromStatus = post.Status;
            post.Status = PostStatus.InReview;
            post.ModifiedDate = DateTime.Now;

            await _postRepo.UpdateAsync(post);
            await LogTransitionAsync(postId, fromStatus, PostStatus.InReview, userId, comment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task ApproveAsync(int postId, string editorId, string? comment = null)
        {
            var post = await _postRepo.GetByIdAsync(postId)
                ?? throw new KeyNotFoundException($"Post {postId} not found");

            if (post.Status != PostStatus.InReview)
                throw new InvalidOperationException("Only posts InReview can be approved.");

            var fromStatus = post.Status;
            post.Status = PostStatus.Published;
            post.ModifiedDate = DateTime.Now;
            post.AssignedEditorId = editorId;

            await _postRepo.UpdateAsync(post);
            await LogTransitionAsync(postId, fromStatus, PostStatus.Published, editorId, comment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task RejectAsync(int postId, string editorId, string comment)
        {
            var post = await _postRepo.GetByIdAsync(postId)
                ?? throw new KeyNotFoundException($"Post {postId} not found");

            if (post.Status != PostStatus.InReview)
                throw new InvalidOperationException("Only posts InReview can be rejected.");

            var fromStatus = post.Status;
            post.Status = PostStatus.Rejected;
            post.ReviewNote = comment;
            post.ModifiedDate = DateTime.Now;
            post.AssignedEditorId = editorId;

            await _postRepo.UpdateAsync(post);
            await LogTransitionAsync(postId, fromStatus, PostStatus.Rejected, editorId, comment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task ScheduleAsync(int postId, string editorId, DateTime publishDate, string? comment = null)
        {
            var post = await _postRepo.GetByIdAsync(postId)
                ?? throw new KeyNotFoundException($"Post {postId} not found");

            if (post.Status != PostStatus.InReview && post.Status != PostStatus.Draft)
                throw new InvalidOperationException("Only Draft or InReview posts can be scheduled.");

            if (publishDate <= DateTime.Now)
                throw new ArgumentException("Scheduled date must be in the future.");

            var fromStatus = post.Status;
            post.Status = PostStatus.Scheduled;
            post.ScheduledPublishDate = publishDate;
            post.ModifiedDate = DateTime.Now;
            post.AssignedEditorId = editorId;

            await _postRepo.UpdateAsync(post);
            await LogTransitionAsync(postId, fromStatus, PostStatus.Scheduled, editorId, comment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<object>> GetPendingReviewsAsync()
        {
            var posts = await _postRepo.GetAllAsync(p => p.Author, p => p.Category);
            return posts
                .Where(p => p.Status == PostStatus.InReview && !p.IsDeleted)
                .OrderByDescending(p => p.ModifiedDate)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Slug,
                    Status = p.Status.ToString(),
                    Category = p.Category?.Name,
                    Author = p.Author?.FullName ?? p.Author?.Email,
                    SubmittedDate = p.ModifiedDate?.ToString("dd/MM/yyyy HH:mm"),
                    p.ReviewNote
                });
        }

        public async Task<IEnumerable<object>> GetWorkflowLogsAsync(int postId)
        {
            var logs = await _workflowLogRepo.FindAsync(l => l.PostId == postId, l => l.User);
            return logs
                .OrderByDescending(l => l.CreatedDate)
                .Select(l => new
                {
                    l.Id,
                    FromStatus = l.FromStatus.ToString(),
                    ToStatus = l.ToStatus.ToString(),
                    User = l.User?.FullName ?? l.User?.Email ?? l.UserId,
                    l.Comment,
                    Date = l.CreatedDate.ToString("dd/MM/yyyy HH:mm")
                });
        }

        private async Task LogTransitionAsync(int postId, PostStatus from, PostStatus to, string userId, string? comment)
        {
            var log = new WorkflowLog
            {
                PostId = postId,
                FromStatus = from,
                ToStatus = to,
                UserId = userId,
                Comment = comment,
                CreatedDate = DateTime.Now
            };
            await _workflowLogRepo.AddAsync(log);
        }
    }
}
