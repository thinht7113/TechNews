using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNews.Domain.Entities;
using TechNews.Infrastructure.Data;

namespace Project_News.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly TechNewsDbContext _context;

        public MenuViewComponent(TechNewsDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = await _context.MenuItems
                .Include(m => m.SubItems)
                .Where(m => m.ParentId == null) // Get root items
                .OrderBy(m => m.Order)
                .ToListAsync();

            return View(items);
        }
    }
}
