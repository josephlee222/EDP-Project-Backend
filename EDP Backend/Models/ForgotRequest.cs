using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class ForgotRequest
    {
        [MaxLength(128), EmailAddress, Required]
        public string Email { get; set; } = string.Empty;
    }
}