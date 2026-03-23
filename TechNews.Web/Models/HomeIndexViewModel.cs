using TechNews.Domain.Entities;
using System;

namespace TechNews.Web.Models
{
    public class HomeIndexViewModel
    {
        public Post FeaturedMain { get; set; }
        public List<Post> FeaturedSub { get; set; } = new();

        public List<Post> LatestStream { get; set; } = new();

        public List<Post> BusinessNews { get; set; } = new();
        public List<Post> TechNews { get; set; } = new();
        public List<Post> MostViewed { get; set; } = new();

        public List<string> TrendingTags { get; set; } = new();
        public List<CategorySection> CategorySections { get; set; } = new();
        public List<Post> LatestPosts { get; set; } = new();
        public List<RecentCommentItem> RecentComments { get; set; } = new();
    }

    public class RecentCommentItem
    {
        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        public string Content { get; set; }
        public string PostTitle { get; set; }
        public string PostSlug { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CategorySection
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategorySlug { get; set; }
        public List<Post> Posts { get; set; } = new();
    }
}