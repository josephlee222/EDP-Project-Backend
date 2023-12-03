using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class Group
    {
        public int Id { get; set; }
        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(1024)]
        public string? Description { get; set; }
        public Boolean IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
