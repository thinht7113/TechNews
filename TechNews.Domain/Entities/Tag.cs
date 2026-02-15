using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TechNews.Domain.Entities
{
    public class Tag : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Slug { get; set; }

        public int Count { get; set; } = 0;

        public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}
