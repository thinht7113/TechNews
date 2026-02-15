using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechNews.Domain.Entities;
using TechNews.Domain.Interfaces;
using System.Threading.Tasks;
using Project_News.Areas.Admin.Models;

namespace Project_News.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Category> _categoryRepo;

        public CategoryController(IUnitOfWork unitOfWork, IRepository<Category> categoryRepo)
        {
            _unitOfWork = unitOfWork;
            _categoryRepo = categoryRepo;
        }

        public IActionResult Index() => View("Spa");
        [Route("Create")]
        public IActionResult Create() => View("Spa");
        [Route("Edit/{id?}")]
        public IActionResult Edit(int id) => View("Spa");

        [HttpGet]
        [Route("api/category/getall")]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryRepo.GetAllAsync();
            return Json(categories);
        }

        [HttpGet]
        [Route("api/category/get/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();
            return Json(category);
        }

        [HttpPost]
        [Route("api/category/create")]
        public async Task<IActionResult> CreateApi([FromBody] Category category)
        {
            if (string.IsNullOrEmpty(category.Slug))
            {
                category.Slug = category.Name.ToLower().Replace(" ", "-");
            }
            category.CreatedDate = System.DateTime.Now;
            
            await _categoryRepo.AddAsync(category);
            await _unitOfWork.CompleteAsync();
            return Ok(category);
        }

        [HttpPost]
        [Route("api/category/update/{id}")]
        public async Task<IActionResult> UpdateApi(int id, [FromBody] Category model)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();

            category.Name = model.Name;
            category.Slug = model.Slug;
            if (string.IsNullOrEmpty(category.Slug)) category.Slug = category.Name.ToLower().Replace(" ", "-");
            
            category.Description = model.Description;
            category.ParentId = model.ParentId;

            await _categoryRepo.UpdateAsync(category);
            await _unitOfWork.CompleteAsync();

            return Ok(category);
        }

        [HttpPost]
        [Route("api/category/delete/{id}")]
        public async Task<IActionResult> DeleteApi(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();

            await _categoryRepo.DeleteAsync(category);
            await _unitOfWork.CompleteAsync();
            return Ok(new { success = true });
        }
    }
}