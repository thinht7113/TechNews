using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TechNews.Application.DTOs;
using TechNews.Domain.Entities;

namespace TechNews.Application.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<object>> GetAllPostsAsync();
        Task<object?> GetPostByIdAsync(int id);
        Task CreatePostAsync(CreatePostDto dto, string authorId);
        Task UpdatePostAsync(int id, EditPostDto dto);
        Task DeletePostAsync(int id);
        Task<bool> RestorePostAsync(int id);
        Task PermanentDeletePostAsync(int id);
        
        Task<IEnumerable<object>> GetTrashAsync();

        Task<IEnumerable<object>> GetRevisionsAsync(int postId);
        Task RestoreRevisionAsync(int revisionId);
        
        Task<string> UploadImageAsync(IFormFile file);
    }
}