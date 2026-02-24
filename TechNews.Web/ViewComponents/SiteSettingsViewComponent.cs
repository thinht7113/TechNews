using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNews.Infrastructure.Data;

namespace TechNews.Web.ViewComponents
{
    public class SiteSettingsViewComponent : ViewComponent
    {
        private readonly TechNewsDbContext _context;

        public SiteSettingsViewComponent(TechNewsDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var settings = await _context.Settings.ToListAsync();
            var dict = settings.ToDictionary(s => s.Key, s => s.Value ?? "");
            return View("Default", dict);
        }
    }
}
