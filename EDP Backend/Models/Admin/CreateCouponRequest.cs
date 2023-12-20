using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models.Admin
{
    public class CreateCouponRequest
    {
        [MaxLength(128), Required]
        public string Code { get; set; } = string.Empty;
        [MaxLength(1024)]
        public string? Description { get; set; }
        [MaxLength(24), Required]
        public string? DiscountType { get; set; }
        [Required]
        public decimal DiscountAmount { get; set; } = 0;
        [Required]
        public DateTime Expiry { get; set; }
    }
}
