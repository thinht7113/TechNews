namespace TechNews.Application.DTOs
{
    public class SeoAnalysisResult
    {
        public int OverallScore { get; set; }
        public SeoCheck TitleAnalysis { get; set; } = new();
        public SeoCheck MetaDescriptionAnalysis { get; set; } = new();
        public SeoCheck KeywordAnalysis { get; set; } = new();
        public SeoCheck ReadabilityAnalysis { get; set; } = new();
        public SeoCheck HeadingAnalysis { get; set; } = new();
        public SeoCheck ImageAnalysis { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
    }

    public class SeoCheck
    {
        public string Status { get; set; } = "warning"; // "good", "warning", "bad"
        public int Score { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class SeoAnalysisRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? FocusKeyword { get; set; }
    }
}
