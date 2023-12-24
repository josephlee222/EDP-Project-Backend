using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models.Admin
{
    public class EditCouponRequest
    {
        [MaxLength(128)]
        public string? Code { get; set; } = string.Empty;
        [MaxLength(1024)]
        public string? Description { get; set; }
        [MaxLength(24)]
        public string? DiscountType { get; set; }
        public decimal? DiscountAmount { get; set; } = 0;
        public DateTime? Expiry { get; set; }
    }
}
