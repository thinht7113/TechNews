using System.Collections.Generic;
using System.Threading.Tasks;
using TechNews.Domain.Entities;

namespace TechNews.Application.Interfaces
{
    public interface IProfileService
    {
        // Saved Posts (Bookmarks)
        Task<bool> IsPostSavedAsync(string userId, int postId);
        Task<bool> ToggleSavePostAsync(string userId, int postId);
        Task<IEnumerable<Post>> GetSavedPostsAsync(string userId, int take = 10, int skip = 0);
        Task<int> GetSavedPostsCountAsync(string userId);

        // Viewed History
        Task RecordPostViewAsync(string userId, int postId);
        Task<IEnumerable<Post>> GetViewHistoryAsync(string userId, int take = 10, int skip = 0);
        Task<int> GetViewHistoryCountAsync(string userId);

        // Comments
        Task<IEnumerable<Comment>> GetUserCommentsAsync(string userId, int take = 10, int skip = 0);
        Task<int> GetUserCommentsCountAsync(string userId);
    }
}
