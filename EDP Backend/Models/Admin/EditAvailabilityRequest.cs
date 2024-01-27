using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models.Admin
{
    public class EditAvailabilityRequest
    {
        [MaxLength(128)]
        public int Id { get; set; }
        public int ActivityId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public float? Price { get; set; }

        //maximum no. of people
        public int? MaxPax { get; set; }

        //current no. of people
        public int? CurrentPax { get; set; }
    }
}
