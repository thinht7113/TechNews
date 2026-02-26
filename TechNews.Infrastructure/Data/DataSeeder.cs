using Microsoft.AspNetCore.Identity;
using TechNews.Domain.Entities;
using TechNews.Domain.Enums;

namespace TechNews.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(TechNewsDbContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
        {

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new Role { Name = "Admin" });
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new Role { Name = "User" });
            }



            var adminUser = new User
            {
                UserName = "admin@technews.com",
                Email = "admin@technews.com",
                FullName = "Administrator",
                EmailConfirmed = true
            };

            var user = await userManager.FindByEmailAsync(adminUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(adminUser, "Admin@123");
                user = await userManager.FindByEmailAsync(adminUser.Email);
            }

            if (!await userManager.IsInRoleAsync(user!, "Admin"))
            {
                await userManager.AddToRoleAsync(user!, "Admin");
            }

            var author = await userManager.FindByEmailAsync(adminUser.Email);


        }
    }
}