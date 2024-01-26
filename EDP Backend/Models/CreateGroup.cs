using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class CreateGroup
    {
        [MaxLength(32),Required]
        public string Name { get; set; } = string.Empty;
        [MaxLength(64)]
        public string Description { get; set; } = string.Empty;
    }
}
