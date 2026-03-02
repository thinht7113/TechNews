using TechNews.Domain.Entities;

namespace TechNews.Domain.Entities
{
    public class StaticPage : BaseEntity
    {
        public string Title { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public string Content { get; set; } = default!;
        public bool IsActive { get; set; } = true;
    }
}
