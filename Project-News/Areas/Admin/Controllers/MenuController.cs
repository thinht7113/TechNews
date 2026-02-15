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
    public class MenuController : Controller
    {
        private readonly TechNewsDbContext _context;

        public MenuController(TechNewsDbContext context)
        {
            _context = context;
        }

        [Route("/Admin/Menu")]
        public IActionResult Index() => View("~/Areas/Admin/Views/Shared/Spa.cshtml");

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.MenuItems
                .Include(m => m.SubItems)
                .Where(m => m.ParentId == null)
                .OrderBy(m => m.Order)
                .ToListAsync();
            return Ok(items);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] MenuItem model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.MenuItems.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MenuItem model)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) return NotFound();

            item.Title = model.Title;
            item.Url = model.Url;
            item.Order = model.Order;
            item.OpenInNewTab = model.OpenInNewTab;
            item.ParentId = model.ParentId;

            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) return NotFound();

            
            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
        
        [HttpPost("updateOrder")]
        public async Task<IActionResult> UpdateOrder([FromBody] List<MenuItemDto> items)
        {
            
            foreach(var item in items)
            {
                 var dbItem = await _context.MenuItems.FindAsync(item.Id);
                 if(dbItem != null)
                 {
                     dbItem.Order = item.Order;
                     dbItem.ParentId = item.ParentId;
                 }
            }
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        public class MenuItemDto
        {
            public int Id { get; set; }
            public int Order { get; set; }
            public int? ParentId { get; set; }
        }
    }
}