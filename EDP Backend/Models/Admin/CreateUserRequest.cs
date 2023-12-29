using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class CreateUserRequest
    {
        [MinLength(3), MaxLength(64), Required]
        [RegularExpression(@"^[a-zA-Z '-,.]+$", ErrorMessage = "Only letters, spaces and characters: ' - , . are accepted")]
        public string Name { get; set; } = string.Empty;
        [MaxLength(128), EmailAddress, Required]
        public string Email { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;
    }
}