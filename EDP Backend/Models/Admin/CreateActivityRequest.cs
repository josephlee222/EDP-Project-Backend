using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models.Admin
{
    public class CreateActivityRequest
    {
        /*[MaxLength(128), Required]
        public string Code { get; set; } = string.Empty;
        [MaxLength(1024)]
        public string? Description { get; set; }
        [MaxLength(24), Required]
        public string? DiscountType { get; set; }
        [Required]
        public decimal DiscountAmount { get; set; } = 0;
        [Required]
        public DateTime Expiry { get; set; }*/
        [MaxLength(128), Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public DateTime ExpiryDate { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool? NtucExclusive { get; set; } = false;
        public int? AgeLimit { get; set; }
        public string? Location { get; set; }
        public string? Company { get; set; }
        public string? DiscountType { get; set; }
        public float? DiscountAmount { get; set; }
        //public array Pictures { get; set; }
    }
}
