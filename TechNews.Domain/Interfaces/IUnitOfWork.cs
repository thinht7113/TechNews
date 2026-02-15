using System;
using System.Threading.Tasks;

namespace TechNews.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> CompleteAsync();
    }
}
