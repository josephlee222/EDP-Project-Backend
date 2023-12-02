using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class LoginRequest
    {
        [MaxLength(128), EmailAddress, Required]
        public string Email { get; set; } = string.Empty;
        [MaxLength(128), Required]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*[0-9]).{8,}$", ErrorMessage = "Requires at least one letter and number")]
        public string Password { get; set; } = string.Empty;
    }
}