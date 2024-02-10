using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class Booking
    {
        [MaxLength(128)]
        public int Id { get; set; }
        public int UserId { get; set; }
        public Availability Availability { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime Date { get; set; }
        public int Pax {  get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
