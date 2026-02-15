using System.Collections.Generic;
using TechNews.Domain.Entities;

namespace TechNews.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public virtual Category Parent { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
