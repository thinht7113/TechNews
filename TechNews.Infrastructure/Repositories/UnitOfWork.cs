using System.Threading.Tasks;
using TechNews.Domain.Interfaces;
using TechNews.Infrastructure.Data;

namespace TechNews.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TechNewsDbContext _context;

        public UnitOfWork(TechNewsDbContext context)
        {
            _context = context;
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
