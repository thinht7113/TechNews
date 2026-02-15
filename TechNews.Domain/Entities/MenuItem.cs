using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechNews.Domain.Entities
{
    public class MenuItem : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        public string Url { get; set; } = "#";

        public int Order { get; set; } = 0;

        public bool OpenInNewTab { get; set; } = false;

        public int? ParentId { get; set; }
        
        [ForeignKey("ParentId")]
        public virtual MenuItem? Parent { get; set; }

        public virtual ICollection<MenuItem> SubItems { get; set; } = new List<MenuItem>();
    }
}
