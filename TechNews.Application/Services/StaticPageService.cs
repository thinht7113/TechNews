using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechNews.Application.Interfaces;
using TechNews.Domain.Entities;
using TechNews.Domain.Interfaces;

namespace TechNews.Application.Services
{
    public class StaticPageService : IStaticPageService
    {
        private readonly IRepository<StaticPage> _repository;

        public StaticPageService(IRepository<StaticPage> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<StaticPage>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<StaticPage>> GetActiveAsync()
        {
            return await _repository.FindAsync(p => p.IsActive);
        }

        public async Task<StaticPage?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<StaticPage?> GetBySlugAsync(string slug)
        {
            var pages = await _repository.FindAsync(p => p.Slug == slug && p.IsActive);
            return pages.FirstOrDefault();
        }

        public async Task<StaticPage> CreateAsync(StaticPage page)
        {
            await _repository.AddAsync(page);
            return page;
        }

        public async Task UpdateAsync(StaticPage page)
        {
            await _repository.UpdateAsync(page);
        }

        public async Task DeleteAsync(int id)
        {
            var page = await _repository.GetByIdAsync(id);
            if (page != null)
            {
                await _repository.DeleteAsync(page);
            }
        }
    }
}
