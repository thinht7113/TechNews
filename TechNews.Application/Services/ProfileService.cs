using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechNews.Application.Interfaces;
using TechNews.Domain.Entities;
using TechNews.Domain.Interfaces;

namespace TechNews.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IRepository<SavedPost> _savedPostRepo;
        private readonly IRepository<UserPostHistory> _historyRepo;
        private readonly IRepository<Comment> _commentRepo;
        private readonly IRepository<Post> _postRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ProfileService(
            IRepository<SavedPost> savedPostRepo,
            IRepository<UserPostHistory> historyRepo,
            IRepository<Comment> commentRepo,
            IRepository<Post> postRepo,
            IUnitOfWork unitOfWork)
        {
            _savedPostRepo = savedPostRepo;
            _historyRepo = historyRepo;
            _commentRepo = commentRepo;
            _postRepo = postRepo;
            _unitOfWork = unitOfWork;
        }

        // --- Saved Posts (Bookmarks) ---
        public async Task<bool> IsPostSavedAsync(string userId, int postId)
        {
            var saved = await _savedPostRepo.FindAsync(s => s.UserId == userId && s.PostId == postId);
            return saved.Any();
        }

        public async Task<bool> ToggleSavePostAsync(string userId, int postId)
        {
            var savedList = await _savedPostRepo.FindAsync(s => s.UserId == userId && s.PostId == postId);
            var saved = savedList.FirstOrDefault();
            
            if (saved != null)
            {
                await _savedPostRepo.DeleteAsync(saved);
                await _unitOfWork.CompleteAsync();
                return false; // Removed
            }
            else
            {
                await _savedPostRepo.AddAsync(new SavedPost { UserId = userId, PostId = postId, SavedAt = DateTime.Now });
                await _unitOfWork.CompleteAsync();
                return true; // Added
            }
        }

        public async Task<IEnumerable<Post>> GetSavedPostsAsync(string userId, int take = 10, int skip = 0)
        {
            var allSaved = await _savedPostRepo.FindAsync(s => s.UserId == userId);
            var savedPostIds = allSaved
                .OrderByDescending(s => s.SavedAt)
                .Skip(skip).Take(take)
                .Select(s => s.PostId)
                .ToList();

            if (!savedPostIds.Any()) return new List<Post>();

            var posts = await _postRepo.GetAllAsync(p => p.Category);
            var filteredPosts = posts.Where(p => savedPostIds.Contains(p.Id)).ToList();

            // Sắp xếp lại đúng thứ tự lưu
            return savedPostIds.Select(id => filteredPosts.FirstOrDefault(p => p.Id == id)).Where(p => p != null).ToList();
        }

        public async Task<int> GetSavedPostsCountAsync(string userId)
        {
            var allSaved = await _savedPostRepo.FindAsync(s => s.UserId == userId);
            return allSaved.Count();
        }

        // --- Viewed History ---
        public async Task RecordPostViewAsync(string userId, int postId)
        {
            var historyList = await _historyRepo.FindAsync(h => h.UserId == userId && h.PostId == postId);
            var history = historyList.FirstOrDefault();
            
            if (history != null)
            {
                history.ViewedAt = DateTime.Now;
                await _historyRepo.UpdateAsync(history);
            }
            else
            {
                await _historyRepo.AddAsync(new UserPostHistory { UserId = userId, PostId = postId, ViewedAt = DateTime.Now });
            }
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<Post>> GetViewHistoryAsync(string userId, int take = 10, int skip = 0)
        {
            var allHistory = await _historyRepo.FindAsync(h => h.UserId == userId);
            var historyPostIds = allHistory
                .OrderByDescending(h => h.ViewedAt)
                .Skip(skip).Take(take)
                .Select(h => h.PostId)
                .ToList();

            if (!historyPostIds.Any()) return new List<Post>();

            var posts = await _postRepo.GetAllAsync(p => p.Category);
            var filteredPosts = posts.Where(p => historyPostIds.Contains(p.Id)).ToList();

            return historyPostIds.Select(id => filteredPosts.FirstOrDefault(p => p.Id == id)).Where(p => p != null).ToList();
        }

        public async Task<int> GetViewHistoryCountAsync(string userId)
        {
            var allHistory = await _historyRepo.FindAsync(h => h.UserId == userId);
            return allHistory.Count();
        }

        // --- Comments ---
        public async Task<IEnumerable<Comment>> GetUserCommentsAsync(string userId, int take = 10, int skip = 0)
        {
            var comments = await _commentRepo.FindAsync(c => c.UserId == userId, c => c.Post);
            
            return comments
                .OrderByDescending(c => c.CreatedDate)
                .Skip(skip).Take(take)
                .ToList();
        }

        public async Task<int> GetUserCommentsCountAsync(string userId)
        {
            var comments = await _commentRepo.FindAsync(c => c.UserId == userId);
            return comments.Count();
        }
    }
}
