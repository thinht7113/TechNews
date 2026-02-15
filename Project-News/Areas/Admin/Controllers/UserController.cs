using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TechNews.Domain.Entities;

namespace Project_News.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public UserController(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index() => View("Spa");
        [Route("Create")]
        public IActionResult CreateView() => View("Spa");
        [Route("Edit/{id?}")]
        public IActionResult EditView(string id) => View("Spa");

        [HttpGet]
        [Route("api/user/getall")]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20)
        {
            var users = await _userManager.Users.ToListAsync();
            var totalCount = users.Count;
            var pagedUsers = users.Skip((page - 1) * pageSize).Take(pageSize);
            var result = pagedUsers.Select(u => new
            {
                u.Id,
                u.Email,
                u.FullName,
                Role = _userManager.GetRolesAsync(u).Result.FirstOrDefault() ?? "User",
                u.CreatedDate,
                IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd.Value > System.DateTimeOffset.Now
            });
            return Ok(new { data = result, totalCount, page, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) });
        }

        [HttpGet]
        [Route("api/user/get/{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            return Ok(new
            {
                user.Id,
                user.Email,
                user.FullName,
                Role = role,
                IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > System.DateTimeOffset.Now
            });
        }

        [HttpPost]
        [Route("api/user/create")]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return BadRequest(new { message = "Email đã tồn tại" });

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                CreatedDate = System.DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(model.Role))
                    await _roleManager.CreateAsync(new Role { Name = model.Role });
                
                await _userManager.AddToRoleAsync(user, model.Role);
                return Ok(user);
            }

            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        [HttpPost]
        [Route("api/user/update/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email;
            user.FullName = model.FullName;
            
            
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                
                if (!await _roleManager.RoleExistsAsync(model.Role))
                     await _roleManager.CreateAsync(new Role { Name = model.Role });
                await _userManager.AddToRoleAsync(user, model.Role);

                return Ok(user);
            }
             return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        [HttpPost]
        [Route("api/user/lock/{id}")]
        public async Task<IActionResult> Lock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            
            var result = await _userManager.SetLockoutEndDateAsync(user, System.DateTimeOffset.Now.AddYears(100));
            if (result.Succeeded) return Ok(new { success = true });
            return BadRequest("Could not lock user");
        }

        [HttpPost]
        [Route("api/user/unlock/{id}")]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            if (result.Succeeded) return Ok(new { success = true });
            return BadRequest("Could not unlock user");
        }

        [HttpPost]
        [Route("api/user/delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            

            var result = await _userManager.DeleteAsync(user);
             if (result.Succeeded) return Ok(new { success = true });
             
             return BadRequest(new { message = "Không thể xóa người dùng này." });
        }

        [HttpPost]
        [Route("api/user/resetpassword/{id}")]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordRequest model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (string.IsNullOrEmpty(model.NewPassword))
                return BadRequest(new { message = "Mật khẩu mới không được để trống" });

            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
                return BadRequest(new { message = "Lỗi khi xóa mật khẩu cũ" });

            var addResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (addResult.Succeeded)
                return Ok(new { success = true });

            return BadRequest(new { message = string.Join(", ", addResult.Errors.Select(e => e.Description)) });
        }

        public class CreateUserRequest
        {
            [Required(ErrorMessage = "Email không được để trống")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Mật khẩu không được để trống")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Họ tên không được để trống")]
            [StringLength(200, ErrorMessage = "Họ tên tối đa 200 ký tự")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Vai trò không được để trống")]
            public string Role { get; set; }
        }
        
        public class UpdateUserRequest
        {
            [Required(ErrorMessage = "Email không được để trống")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Họ tên không được để trống")]
            [StringLength(200, ErrorMessage = "Họ tên tối đa 200 ký tự")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Vai trò không được để trống")]
            public string Role { get; set; }
        }

        public class ResetPasswordRequest
        {
            [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự")]
            public string NewPassword { get; set; }
        }
    }
}