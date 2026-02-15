using TechNews.Domain.Entities;

namespace Project_News.Models
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
    }

    public class CategorySection
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategorySlug { get; set; }
        public List<Post> Posts { get; set; } = new();
    }
}