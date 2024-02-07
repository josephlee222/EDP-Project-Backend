using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class MakeCredentialsOptionsRequest
    {
        [MaxLength(128),Required]
        public string Password { get; set; } = string.Empty;
    }
}
