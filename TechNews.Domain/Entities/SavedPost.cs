using System;
using System.ComponentModel.DataAnnotations;

namespace TechNews.Domain.Entities
{
    public class SavedPost
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public virtual User User { get; set; }
        
        [Required]
        public int PostId { get; set; }
        public virtual Post Post { get; set; }
        
        public DateTime SavedAt { get; set; } = DateTime.Now;
    }
}
