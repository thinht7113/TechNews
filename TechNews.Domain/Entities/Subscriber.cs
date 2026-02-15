using System.ComponentModel.DataAnnotations;

namespace TechNews.Domain.Entities
{
    public class Subscriber : BaseEntity
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime? UnsubscribedDate { get; set; }
    }
}
