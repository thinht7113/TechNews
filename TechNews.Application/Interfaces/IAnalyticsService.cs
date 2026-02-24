namespace TechNews.Application.Interfaces
{
    public interface IAnalyticsService
    {
        Task TrackPageViewAsync(int postId, string? sessionId, string? ipAddress, string? userAgent, string? referrer, int timeOnPage = 0, int scrollDepth = 0);
        Task<object> GetPostAnalyticsAsync(int postId, int days = 30);
        Task<object> GetOverviewAnalyticsAsync(int days = 30);
    }
}
