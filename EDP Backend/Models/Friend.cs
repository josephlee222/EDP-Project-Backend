using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class Friend
    {
        public int Id { get; set; }
		[Required]
		public int SenderID { get; set; }
		[Required]
		public int RecipientID { get; set; }
		[Required]
		public string Name { get; set; } = string.Empty;
		[Required]
		public string? ProfilePicture { get; set; }
		public DateTime AddedAt { get; set; } = DateTime.Now;
    }
}
