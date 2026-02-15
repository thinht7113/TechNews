using System.ComponentModel.DataAnnotations;
using TechNews.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace TechNews.Application.DTOs
{
    public class CreatePostDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string ShortDescription { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public PostStatus Status { get; set; } = PostStatus.Draft;

        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? Tags { get; set; }

        [Display(Name = "Thumbnail Image")]
        public IFormFile? ThumbnailFile { get; set; }
        
        [Display(Name = "Or Image URL")]
        public string? ThumbnailUrl { get; set; }
    }

    public class EditPostDto : CreatePostDto
    {
        public int Id { get; set; }
        public string? ExistingThumbnail { get; set; }
    }
}