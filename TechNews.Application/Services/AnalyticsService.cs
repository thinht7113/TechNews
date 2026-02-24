using TechNews.Application.Interfaces;
using TechNews.Domain.Entities;
using TechNews.Domain.Interfaces;

namespace TechNews.Application.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IRepository<PageView> _pageViewRepo;
        private readonly IRepository<Post> _postRepo;
        private readonly IUnitOfWork _unitOfWork;

        public AnalyticsService(
            IRepository<PageView> pageViewRepo,
            IRepository<Post> postRepo,
            IUnitOfWork unitOfWork)
        {
            _pageViewRepo = pageViewRepo;
            _postRepo = postRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task TrackPageViewAsync(int postId, string? sessionId, string? ipAddress, string? userAgent, string? referrer, int timeOnPage = 0, int scrollDepth = 0)
        {
            var pageView = new PageView
            {
                PostId = postId,
                SessionId = sessionId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Referrer = referrer,
                TimeOnPage = timeOnPage,
                ScrollDepth = scrollDepth,
                CreatedDate = DateTime.Now
            };

            await _pageViewRepo.AddAsync(pageView);

            // Also increment ViewCount on Post
            var post = await _postRepo.GetByIdAsync(postId);
            if (post != null)
            {
                post.ViewCount++;
                await _postRepo.UpdateAsync(post);
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task<object> GetPostAnalyticsAsync(int postId, int days = 30)
        {
            var since = DateTime.Now.AddDays(-days);
            var views = (await _pageViewRepo.FindAsync(pv => pv.PostId == postId && pv.CreatedDate >= since)).ToList();

            var dailyViews = Enumerable.Range(0, days)
                .Select(i => DateTime.Today.AddDays(-i))
                .Reverse()
                .Select(date => new
                {
                    Date = date.ToString("dd/MM"),
                    Views = views.Count(v => v.CreatedDate.Date == date)
                });

            var uniqueVisitors = views.Select(v => v.IpAddress).Distinct().Count();
            var avgTimeOnPage = views.Any() ? views.Average(v => v.TimeOnPage) : 0;
            var avgScrollDepth = views.Any() ? views.Average(v => v.ScrollDepth) : 0;
            var bounceRate = views.Any() ? (double)views.Count(v => v.TimeOnPage < 10) / views.Count * 100 : 0;

            var topReferrers = views
                .Where(v => !string.IsNullOrEmpty(v.Referrer))
                .GroupBy(v => v.Referrer)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Source = g.Key, Count = g.Count() });

            return new
            {
                TotalViews = views.Count,
                UniqueVisitors = uniqueVisitors,
                AvgTimeOnPage = Math.Round(avgTimeOnPage, 1),
                AvgScrollDepth = Math.Round(avgScrollDepth, 1),
                BounceRate = Math.Round(bounceRate, 1),
                DailyViews = dailyViews,
                TopReferrers = topReferrers
            };
        }

        public async Task<object> GetOverviewAnalyticsAsync(int days = 30)
        {
            var since = DateTime.Now.AddDays(-days);
            var views = (await _pageViewRepo.FindAsync(pv => pv.CreatedDate >= since)).ToList();

            var totalViews = views.Count;
            var uniqueVisitors = views.Select(v => v.IpAddress).Distinct().Count();
            var avgTimeOnPage = views.Any() ? views.Average(v => v.TimeOnPage) : 0;
            var bounceRate = views.Any() ? (double)views.Count(v => v.TimeOnPage < 10) / views.Count * 100 : 0;

            var dailyViews = Enumerable.Range(0, days)
                .Select(i => DateTime.Today.AddDays(-i))
                .Reverse()
                .Select(date => new
                {
                    Date = date.ToString("dd/MM"),
                    Views = views.Count(v => v.CreatedDate.Date == date)
                });

            var topPosts = views
                .GroupBy(v => v.PostId)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new
                {
                    PostId = g.Key,
                    Views = g.Count(),
                    AvgTime = Math.Round(g.Average(v => v.TimeOnPage), 1)
                });

            var topReferrers = views
                .Where(v => !string.IsNullOrEmpty(v.Referrer))
                .GroupBy(v => v.Referrer)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new { Source = g.Key, Count = g.Count() });

            return new
            {
                TotalViews = totalViews,
                UniqueVisitors = uniqueVisitors,
                AvgTimeOnPage = Math.Round(avgTimeOnPage, 1),
                BounceRate = Math.Round(bounceRate, 1),
                DailyViews = dailyViews,
                TopPosts = topPosts,
                TopReferrers = topReferrers
            };
        }
    }
}
