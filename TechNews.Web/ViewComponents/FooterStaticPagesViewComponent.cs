using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TechNews.Application.Interfaces;

namespace TechNews.Web.ViewComponents
{
    public class FooterStaticPagesViewComponent : ViewComponent
    {
        private readonly IStaticPageService _pageService;

        public FooterStaticPagesViewComponent(IStaticPageService pageService)
        {
            _pageService = pageService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var pages = await _pageService.GetActiveAsync();
            return View(pages);
        }
    }
}
