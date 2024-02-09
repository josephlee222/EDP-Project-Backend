using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class Availability
    {

        [MaxLength(128)]
        public int Id { get; set; }
        public Activity Activity { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime Date {  get; set; }
        public float Price { get; set; }

        //maximum no. of people
        public int MaxPax { get; set; }
        public int CurrentPax { get; set; }

        // Collection of bookings made for this availability
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
