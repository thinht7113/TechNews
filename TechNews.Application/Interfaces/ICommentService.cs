using TechNews.Domain.Entities;

namespace TechNews.Application.Interfaces
{
    public interface ICommentService
    {
        Task<(IEnumerable<object> Data, int TotalCount)> GetAllCommentsAsync(int page = 1, int pageSize = 20);
        Task ApproveCommentAsync(int id);
        Task DeleteCommentAsync(int id);
        Task ReplyToCommentAsync(int parentId, string content, string userId);
    }
}
