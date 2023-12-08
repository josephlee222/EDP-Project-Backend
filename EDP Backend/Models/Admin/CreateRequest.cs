using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class CreateRequest
    {
        [MinLength(3), MaxLength(64), Required]
        [RegularExpression(@"^[a-zA-Z '-,.]+$", ErrorMessage = "Only allow letters, spaces and characters: ' - , .")]
        public string Name { get; set; } = string.Empty;
        [MaxLength(128), EmailAddress, Required]
        public string Email { get; set; } = string.Empty;
    }
}