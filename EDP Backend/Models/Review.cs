using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class Review
    {
        [MaxLength(128)]
        public int Id { get; set; }
        public User User { get; set; }
        public int ActivityId { get; set; }
        public int Rating { get; set; }
        public string Description { get; set; } = string.Empty;
        public StringArray? Pictures { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
