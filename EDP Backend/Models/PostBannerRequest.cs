using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class PostBannerRequest
    {
        [MaxLength(128), Required]
		public string ImagePath { get; set; } = string.Empty;
		[Required]
		public int Slot { get; set; } = 1;
	}
}