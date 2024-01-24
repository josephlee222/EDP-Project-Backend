using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class EditGroupRequest
    {
        [MaxLength(32)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(64)]
        public string Description { get; set; } = string.Empty;
    }
}
