using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechNews.Domain.Entities
{
    public class PostTag
    {
        [Key]
        public int Id { get; set; }

        public int PostId { get; set; }
        public virtual Post Post { get; set; }

        public int TagId { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
