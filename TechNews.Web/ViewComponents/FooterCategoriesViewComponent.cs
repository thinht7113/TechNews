using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNews.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace TechNews.Web.ViewComponents
{
    public class FooterCategoriesViewComponent : ViewComponent
    {
        private readonly TechNewsDbContext _db;

        public FooterCategoriesViewComponent(TechNewsDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var topCategories = await _db.Categories
                .Where(c => c.ParentId == null) // only top level
                .OrderByDescending(c => c.Posts.Count)
                .Take(5)
                .ToListAsync();

            return View(topCategories);
        }
    }
}
