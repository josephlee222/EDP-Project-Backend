using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Availability Availability { get; set; }
        public int Pax { get; set; }
    }
}
