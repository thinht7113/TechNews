using TechNews.Application.Interfaces;
using TechNews.Domain.Entities;
using TechNews.Domain.Interfaces;

namespace TechNews.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepo;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IRepository<Category> categoryRepo, IUnitOfWork unitOfWork)
        {
            _categoryRepo = categoryRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepo.GetAllAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            var categories = await _categoryRepo.FindAsync(c => c.Id == id);
            return categories.FirstOrDefault();
        }

        public async Task<Category> CreateCategoryAsync(string name, string slug, string? description)
        {
            var category = new Category
            {
                Name = name,
                Slug = slug,
                Description = description,
                CreatedDate = DateTime.Now
            };

            await _categoryRepo.AddAsync(category);
            await _unitOfWork.CompleteAsync();
            return category;
        }

        public async Task UpdateCategoryAsync(int id, string name, string slug, string? description)
        {
            var categories = await _categoryRepo.FindAsync(c => c.Id == id);
            var category = categories.FirstOrDefault();
            if (category == null) throw new KeyNotFoundException("Không tìm thấy danh mục");

            category.Name = name;
            category.Slug = slug;
            category.Description = description;

            await _categoryRepo.UpdateAsync(category);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var categories = await _categoryRepo.FindAsync(c => c.Id == id);
            var category = categories.FirstOrDefault();
            if (category == null) throw new KeyNotFoundException("Không tìm thấy danh mục");

            await _categoryRepo.DeleteAsync(category);
            await _unitOfWork.CompleteAsync();
        }
    }
}
