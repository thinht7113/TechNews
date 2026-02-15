using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Project_News.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MediaController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public MediaController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index()
        {
            return View("Spa");
        }

        [HttpGet]
        [Route("api/media/getall")]
        public IActionResult GetAll()
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var files = Directory.GetFiles(uploadsFolder, "*.*", SearchOption.AllDirectories)
                .Select(f => new
                {
                    Name = Path.GetFileName(f),
                    Url = f.Replace(_env.WebRootPath, "").Replace("\\", "/"),
                    Size = new FileInfo(f).Length,
                    Date = System.IO.File.GetLastWriteTime(f)
                })
                .OrderByDescending(f => f.Date)
                .ToList();

            return Json(files);
        }

        [HttpPost]
        [Route("api/media/upload")]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            if (files == null || files.Count == 0) return BadRequest("No files selected");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "library");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            foreach (var file in files)
            {
                var uniqueName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + file.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            return Ok(new { success = true });
        }

        [HttpPost]
        [Route("api/media/delete")]
        public IActionResult Delete([FromBody] DeleteMediaRequest req)
        {
            if (string.IsNullOrEmpty(req.Url)) return BadRequest();

            var relativePath = req.Url.TrimStart('/');
            var fullPath = Path.Combine(_env.WebRootPath, relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
                return Ok(new { success = true });
            }
            return NotFound(new { success = false, message = "File not found" });
        }
    }

    public class DeleteMediaRequest { public string Url { get; set; } }
}