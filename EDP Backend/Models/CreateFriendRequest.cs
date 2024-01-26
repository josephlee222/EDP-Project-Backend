using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class CreateFriendRequest
    {
        [Required]
        public int SenderID { get; set; }
        [Required]
        public int RecipientID { get; set; }
    }
}
