using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechNews.Domain.Entities
{
    public class PostRevision
    {
        [Key]
        public int Id { get; set; }

        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        public string ShortDescription { get; set; }

        public string Content { get; set; }

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        public int Version { get; set; }
    }
}
