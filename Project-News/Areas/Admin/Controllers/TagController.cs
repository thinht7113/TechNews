using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNews.Domain.Entities;
using TechNews.Infrastructure.Data;

namespace Project_News.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : Controller
    {
        private readonly TechNewsDbContext _context;

        public TagController(TechNewsDbContext context)
        {
            _context = context;
        }

        [Route("/Admin/Tag")]
        public IActionResult Index() => View("~/Areas/Admin/Views/Shared/Spa.cshtml");

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var tags = await _context.Tags
                .Select(t => new 
                {
                    t.Id,
                    t.Name,
                    t.Slug,
                    Count = t.PostTags.Count()
                })
                .OrderByDescending(t => t.Count)
                .ToListAsync();
            return Ok(tags);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] Tag model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Tags.AnyAsync(t => t.Slug == model.Slug))
                return BadRequest("Tag already exists");

            _context.Tags.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Tag model)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null) return NotFound();

            tag.Name = model.Name;
            tag.Slug = model.Slug;
            
            await _context.SaveChangesAsync();
            return Ok(tag);
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null) return NotFound();

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}