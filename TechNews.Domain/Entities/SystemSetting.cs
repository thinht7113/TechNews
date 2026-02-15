using System.ComponentModel.DataAnnotations;

namespace TechNews.Domain.Entities
{
    public class SystemSetting
    {
        [Key]
        public string Key { get; set; } // e.g., "SiteTitle", "AdminEmail"
        public string? Value { get; set; }
        public string? DisplayName { get; set; } // For UI Label
        public string? GroupName { get; set; } // e.g., "General", "Social", "SEO"
        public string? Type { get; set; } // "text", "textarea", "boolean", "image"
    }
}
