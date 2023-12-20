using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        [MaxLength(128)]
        public string Code { get; set; } = string.Empty;
        [MaxLength(1024)]
        public string? Description { get; set; }
        public Boolean IsDeleted { get; set; } = false;
        [Column(TypeName = "datetime")]
        public DateTime Expiry { get; set; } = DateTime.Now;
        public string DiscountType { get; set; } = string.Empty;
        [Column(TypeName = "decimal(6,2)")]
        public decimal DiscountAmount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
