using System.ComponentModel.DataAnnotations;

namespace Project_News.Areas.Admin.Models
{
    public class CreateCategoryViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
    }

    public class EditCategoryViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public int? ParentId { get; set; }
    }
}
