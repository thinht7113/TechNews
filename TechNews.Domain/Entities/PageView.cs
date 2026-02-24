namespace TechNews.Domain.Entities
{
    public class PageView
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public virtual Post Post { get; set; } = null!;

        public string? SessionId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Referrer { get; set; }

        public int TimeOnPage { get; set; } // seconds
        public int ScrollDepth { get; set; } // percentage 0-100

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
