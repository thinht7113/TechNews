using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TechNews.Application.Interfaces;
using TechNews.Domain.Entities;

namespace TechNews.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor")]
    [Route("api/admin/staticpages")]
    [ApiController]
    public class StaticPageController : ControllerBase
    {
        private readonly IStaticPageService _service;

        public StaticPageController(IStaticPageService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var page = await _service.GetByIdAsync(id);
            if (page == null) return NotFound();
            return Ok(page);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StaticPage page)
        {
            var created = await _service.CreateAsync(page);
            return Ok(created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] StaticPage page)
        {
            page.Id = id;
            await _service.UpdateAsync(page);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
