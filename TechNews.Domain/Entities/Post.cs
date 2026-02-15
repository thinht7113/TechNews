using System;
using TechNews.Domain.Entities;
using TechNews.Domain.Enums;

namespace TechNews.Domain.Entities
{
    public class Post : BaseEntity
    {
        public string Title { get; set; }
        public string Slug { get; set; }
        public string ShortDescription { get; set; }
        public string Content { get; set; }
        public string? Thumbnail { get; set; }
        public int ViewCount { get; set; } = 0;
        
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? Tags { get; set; }

        public PostStatus Status { get; set; } = PostStatus.Draft;
        public bool IsDeleted { get; set; } = false;
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        
        public string AuthorId { get; set; }
        public virtual User Author { get; set; }
        
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}