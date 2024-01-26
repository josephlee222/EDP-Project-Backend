using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class FriendRequest
    {
        public int Id { get; set; }
        [Required]
        public int SenderID {  get; set; }
        [Required]
        public int RecipientID { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
