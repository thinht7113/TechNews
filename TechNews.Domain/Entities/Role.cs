using Microsoft.AspNetCore.Identity;

namespace TechNews.Domain.Entities
{
    public class Role : IdentityRole
    {
        public string? Description { get; set; }
    }
}
