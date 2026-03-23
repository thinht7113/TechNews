using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechNews.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace TechNews.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _cache;

        public AuthController(SignInManager<User> signInManager, UserManager<User> userManager, IMemoryCache cache)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _cache = cache;
        }

        // Simple rate limiting: max attempts per IP per time window
        private bool IsRateLimited(string action)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var cacheKey = $"RateLimit_{action}_{ip}";
            var attempts = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return 0;
            });

            if (attempts >= 10) // Max 10 attempts per 5 minutes
                return true;

            _cache.Set(cacheKey, attempts + 1, TimeSpan.FromMinutes(5));
            return false;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            // Bug #1 fix: Rate limiting
            if (IsRateLimited("login"))
                return StatusCode(429, new { success = false, message = "Bạn đã thử quá nhiều lần. Vui lòng đợi 5 phút." });

            if (!ModelState.IsValid) 
            {
                return BadRequest(new { success = false, message = "Dữ liệu nhập vào chưa đầy đủ." });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new { success = false, message = "Email hoặc mật khẩu không chính xác." });
            }
            
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            
            if (result.Succeeded) 
            {
                await _signInManager.SignInAsync(user, isPersistent: request.RememberMe);
                return Ok(new { success = true });
            }
            
            if (result.IsLockedOut)
            {
                return BadRequest(new { success = false, message = "Tài khoản của bạn đã bị khóa tạm thời." });
            }
            return BadRequest(new { success = false, message = "Email hoặc mật khẩu không chính xác." });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            // Bug #1 fix: Rate limiting
            if (IsRateLimited("register"))
                return StatusCode(429, new { success = false, message = "Bạn đã thử quá nhiều lần. Vui lòng đợi 5 phút." });

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Dữ liệu nhập vào chưa hợp lệ." });
            }

            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest(new { success = false, message = "Mật khẩu xác nhận không khớp." });
            }

            var user = new User { UserName = request.Email, Email = request.Email, FullName = request.FullName, CreatedDate = DateTime.UtcNow };
            var result = await _userManager.CreateAsync(user, request.Password);
            
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Ok(new { success = true });
            }

            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            return BadRequest(new { success = false, message = errors });
        }
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
