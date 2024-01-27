using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public User User { get; set; }
        [MaxLength(256)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(256)]
        public string? Subtitle { get; set; }
        [MaxLength(64)]
        public string Type { get; set; } = "info";
        [MaxLength(64)]
        public string Action { get; set; } = string.Empty;
        [MaxLength(512)]
        public string? ActionUrl { get; set; }
        public Boolean Read { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
