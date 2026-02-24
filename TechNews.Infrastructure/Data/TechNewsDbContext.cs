using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechNews.Domain.Entities;

namespace TechNews.Infrastructure.Data
{
    public class TechNewsDbContext : IdentityDbContext<User, Role, string>
    {
        public TechNewsDbContext(DbContextOptions<TechNewsDbContext> options) : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
        public DbSet<SystemSetting> Settings { get; set; }
        public DbSet<PostRevision> PostRevisions { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }
        public DbSet<WorkflowLog> WorkflowLogs { get; set; }
        public DbSet<PageView> PageViews { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(250);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(250);
                
                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Posts)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Author)
                    .WithMany()
                    .HasForeignKey(d => d.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.AssignedEditor)
                    .WithMany()
                    .HasForeignKey(d => d.AssignedEditorId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
            });

            builder.Entity<WorkflowLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(d => d.Post)
                    .WithMany(p => p.WorkflowLogs)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.PostId, e.CreatedDate });
            });

            builder.Entity<PageView>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(d => d.Post)
                    .WithMany()
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.PostId, e.CreatedDate });
            });

            builder.Entity<Category>(entity =>
            {
                 entity.HasKey(e => e.Id);
                 entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                 
                 entity.HasOne(d => d.Parent)
                     .WithMany(p => p.SubCategories)
                     .HasForeignKey(d => d.ParentId)
                     .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}