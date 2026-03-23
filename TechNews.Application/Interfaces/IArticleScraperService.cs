namespace TechNews.Application.Interfaces
{
    public interface IArticleScraperService
    {
        Task<ScrapedArticleResult> ScrapeAsync(string url);
    }

    public class ScrapedArticleResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string Title { get; set; } = "";
        public string ShortDescription { get; set; } = "";
        public string Content { get; set; } = "";
        public string ThumbnailUrl { get; set; } = "";
        public string Tags { get; set; } = "";
        public string SourceUrl { get; set; } = "";
    }
}
