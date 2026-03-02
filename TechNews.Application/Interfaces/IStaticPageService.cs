using System.Collections.Generic;
using System.Threading.Tasks;
using TechNews.Domain.Entities;

namespace TechNews.Application.Interfaces
{
    public interface IStaticPageService
    {
        Task<IEnumerable<StaticPage>> GetAllAsync();
        Task<IEnumerable<StaticPage>> GetActiveAsync();
        Task<StaticPage?> GetByIdAsync(int id);
        Task<StaticPage?> GetBySlugAsync(string slug);
        Task<StaticPage> CreateAsync(StaticPage page);
        Task UpdateAsync(StaticPage page);
        Task DeleteAsync(int id);
    }
}
