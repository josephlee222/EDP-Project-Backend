using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class VerifyRequest
    {
        [MaxLength(128), Required]
        public string Token { get; set; } = string.Empty;
    }
}