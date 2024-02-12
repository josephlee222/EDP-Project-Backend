using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class GroupMessage
    {
        public int Id { get; set; }
        [MaxLength(32)]
        public User User { get; set; }
        [MaxLength(64)]
        public Group Group { get; set; }
        [MaxLength(2048)]
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
