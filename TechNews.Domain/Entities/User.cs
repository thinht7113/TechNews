using Microsoft.AspNetCore.Identity;
using System;

namespace TechNews.Domain.Entities
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
