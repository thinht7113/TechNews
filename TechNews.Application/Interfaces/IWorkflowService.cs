using TechNews.Domain.Entities;

namespace TechNews.Application.Interfaces
{
    public interface IWorkflowService
    {
        Task SubmitForReviewAsync(int postId, string userId, string? comment = null);
        Task ApproveAsync(int postId, string editorId, string? comment = null);
        Task RejectAsync(int postId, string editorId, string comment);
        Task ScheduleAsync(int postId, string editorId, DateTime publishDate, string? comment = null);
        Task<IEnumerable<object>> GetPendingReviewsAsync();
        Task<IEnumerable<object>> GetWorkflowLogsAsync(int postId);
    }
}
