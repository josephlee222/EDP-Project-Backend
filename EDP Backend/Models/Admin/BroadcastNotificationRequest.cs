using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class BroadcastNotificationRequest
    {
        [MaxLength(256), Required]
		public string Title { get; set; } = string.Empty;
        [MaxLength(256), Required]
		public string Subtitle { get; set; } = string.Empty;
        [MaxLength(64), Required]
        public string Action { get; set; } = string.Empty;
        [MaxLength(512), Required]
        public string ActionUrl { get; set; } = string.Empty;
	}
}