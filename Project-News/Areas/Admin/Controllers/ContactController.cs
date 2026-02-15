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
    public class ContactController : Controller
    {
        private readonly TechNewsDbContext _context;

        public ContactController(TechNewsDbContext context)
        {
            _context = context;
        }

        [Route("/Admin/Contact")]
        public IActionResult Index() => View("~/Areas/Admin/Views/Shared/Spa.cshtml");

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var contacts = await _context.Contacts
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
            return Ok(contacts);
        }

        [HttpPost("markread/{id}")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null) return NotFound();

            contact.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null) return NotFound();

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
