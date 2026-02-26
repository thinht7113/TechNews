using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TechNews.Application.DTOs;
using TechNews.Domain.Entities;

namespace TechNews.Application.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<object>> GetAllPostsAsync(string? userId = null, bool isAdmin = false);
        Task<object?> GetPostByIdAsync(int id, string? userId = null, bool isAdmin = false);
        Task CreatePostAsync(CreatePostDto dto, string authorId);
        Task UpdatePostAsync(int id, EditPostDto dto, string? userId = null, bool isAdmin = false);
        Task DeletePostAsync(int id, string? userId = null, bool isAdmin = false);
        Task<bool> RestorePostAsync(int id, string? userId = null, bool isAdmin = false);
        Task PermanentDeletePostAsync(int id, string? userId = null, bool isAdmin = false);
        
        Task<IEnumerable<object>> GetTrashAsync(string? userId = null, bool isAdmin = false);

        Task<IEnumerable<object>> GetRevisionsAsync(int postId);
        Task RestoreRevisionAsync(int revisionId, string? userId = null, bool isAdmin = false);
        
        Task<string> UploadImageAsync(IFormFile file);
    }
}