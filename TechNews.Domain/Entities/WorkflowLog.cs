using TechNews.Domain.Enums;

namespace TechNews.Domain.Entities
{
    public class WorkflowLog
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public virtual Post Post { get; set; } = null!;

        public PostStatus FromStatus { get; set; }
        public PostStatus ToStatus { get; set; }

        public string UserId { get; set; } = string.Empty;
        public virtual User User { get; set; } = null!;

        public string? Comment { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
