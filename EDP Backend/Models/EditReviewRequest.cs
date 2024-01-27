using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class EditReviewRequest
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int ActivityId { get; set; }
        [Required]
        public int Rating { get; set; }
        public string? Description { get; set; } = string.Empty;
    }
}
