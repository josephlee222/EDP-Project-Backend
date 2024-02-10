using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class CheckoutRequest
    {
        //email
        //phone
        //name
        //birthday
        //nric
        //coupon

        [Required]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime Birthday { get; set; }
        [Required]
        public string Nric { get; set; }
        public string? Coupon { get; set; }
    }
}
