using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class SendMessageRequest
	{
		public int GroupId { get; set; }
		[MaxLength(2048), Required]
		public string Message { get; set; } = string.Empty;
	}
}