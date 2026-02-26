namespace TechNews.Application.Interfaces
{
    public interface IAiService
    {
        Task<string> GenerateContentAsync(string prompt, string? context = null);
        Task<string> SummarizeAsync(string content, int maxLength = 200);
        Task<List<string>> SuggestTagsAsync(string content);
        Task<string> ImproveWritingAsync(string content);
        Task<List<string>> GenerateTitlesAsync(string content);
        Task<bool> IsConfiguredAsync();
    }
}
