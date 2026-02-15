using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using TechNews.Infrastructure.Data;

namespace Project_News.Data // Namespace matches project
{
    public class TechNewsDbContextFactory : IDesignTimeDbContextFactory<TechNewsDbContext>
    {
        public TechNewsDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<TechNewsDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlServer(connectionString);

            return new TechNewsDbContext(builder.Options);
        }
    }
}
