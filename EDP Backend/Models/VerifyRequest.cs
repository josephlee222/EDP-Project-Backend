using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class VerifyRequest
    {
        [MaxLength(128), Required]
        public string Token { get; set; } = string.Empty;
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*[0-9]).{8,}$", ErrorMessage = "Requires at least one letter and number")]
        public string Password { get; set; } = string.Empty;
    }
}