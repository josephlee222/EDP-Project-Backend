using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class OAuthRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }
}