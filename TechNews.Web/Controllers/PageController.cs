using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TechNews.Application.Interfaces;

namespace TechNews.Web.Controllers
{
    public class PageController : Controller
    {
        private readonly IStaticPageService _pageService;

        public PageController(IStaticPageService pageService)
        {
            _pageService = pageService;
        }

        [Route("page/{slug}")]
        public async Task<IActionResult> Detail(string slug)
        {
            var page = await _pageService.GetBySlugAsync(slug);
            if (page == null)
            {
                return NotFound();
            }
            return View(page);
        }
    }
}
