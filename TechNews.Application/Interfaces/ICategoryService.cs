using TechNews.Domain.Entities;

namespace TechNews.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category> CreateCategoryAsync(string name, string slug, string? description);
        Task UpdateCategoryAsync(int id, string name, string slug, string? description);
        Task DeleteCategoryAsync(int id);
    }
}
