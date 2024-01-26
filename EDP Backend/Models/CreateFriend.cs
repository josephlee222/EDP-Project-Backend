using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class CreateFriend
    {
        [Required]
        public int FriendID { get; set; }
    }
}
