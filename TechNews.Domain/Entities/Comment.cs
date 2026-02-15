using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechNews.Domain.Entities
{
    public class Comment : BaseEntity
    {
        [Required]
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsApproved { get; set; } = false;
        public int? ParentId { get; set; }

        [Required]
        public int PostId { get; set; }
        public virtual Post Post { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual User User { get; set; }
    }
}