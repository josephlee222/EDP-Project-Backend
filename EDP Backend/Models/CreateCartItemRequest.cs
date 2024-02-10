using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class CreateCartItemRequest
    {
        [Required]
        public int AvailabilityId { get; set; }
        [Required]
        public int Pax { get; set; }
    }
}