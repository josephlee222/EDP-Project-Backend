using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class MakeCredentialsOptionsRequest
    {
        [MaxLength(128),Required]
        public string Username { get; set; } = string.Empty;
        [MaxLength(128),Required]
        public string DisplayName { get; set; } = string.Empty;
        [MaxLength(128),Required]
        public string AttType { get; set; } = string.Empty;
        [MaxLength(128),Required]
        public string AuthType { get; set; } = string.Empty;
        [MaxLength(128),Required]
        public string ResidentKey { get; set; } = string.Empty;
        [MaxLength(128),Required]
        public string UserVerification { get; set; } = string.Empty;
    }
}
