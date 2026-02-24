using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TechNews.Domain.Entities;
using TechNews.Domain.Enums;
using TechNews.Domain.Interfaces;

namespace TechNews.Web.Services
{
    public class ScheduledPublishService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScheduledPublishService> _logger;

        public ScheduledPublishService(IServiceProvider serviceProvider, ILogger<ScheduledPublishService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ScheduledPublishService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PublishScheduledPosts();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ScheduledPublishService.");
                }

                // Check every minute
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task PublishScheduledPosts()
        {
            using var scope = _serviceProvider.CreateScope();
            var postRepo = scope.ServiceProvider.GetRequiredService<IRepository<Post>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var workflowLogRepo = scope.ServiceProvider.GetRequiredService<IRepository<WorkflowLog>>();

            var scheduledPosts = await postRepo.FindAsync(
                p => p.Status == PostStatus.Scheduled
                     && p.ScheduledPublishDate != null
                     && p.ScheduledPublishDate <= DateTime.Now
                     && !p.IsDeleted
            );

            foreach (var post in scheduledPosts)
            {
                var fromStatus = post.Status;
                post.Status = PostStatus.Published;
                post.ModifiedDate = DateTime.Now;
                await postRepo.UpdateAsync(post);

                // Log the auto-publish
                var log = new WorkflowLog
                {
                    PostId = post.Id,
                    FromStatus = fromStatus,
                    ToStatus = PostStatus.Published,
                    UserId = post.AssignedEditorId ?? post.AuthorId,
                    Comment = "Tự động xuất bản theo lịch.",
                    CreatedDate = DateTime.Now
                };
                await workflowLogRepo.AddAsync(log);

                _logger.LogInformation("Auto-published post {PostId}: {Title}", post.Id, post.Title);
            }

            if (scheduledPosts.Any())
            {
                await unitOfWork.CompleteAsync();
            }
        }
    }
}
