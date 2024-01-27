using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class Friend
    {
        public int Id { get; set; }
        [Required]
        public int FriendID { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;
    }
}
