using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class Activity
    {
        [MaxLength(128)]
        public int Id { get; set; }
        [MaxLength(1024)]
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "datetime")]
        public DateTime ExpiryDate { get; set; }
        public string? Description { get; set; } = string.Empty;
        public string? Category { get; set; } = string.Empty;
        public bool? NtucExclusive { get; set; } = false;
        public int? AgeLimit { get; set; }
        public string? Location {  get; set; } = string.Empty;
        public string? Company { get; set; } = string.Empty;
        public string? DiscountType {  get; set; } = string.Empty;
        [Column(TypeName = "decimal(6,2)")]
        public float? DiscountAmount { get; set; } = 0;
        //public array Pictures { get; set; }
        [Column(TypeName = "datetime")]

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column(TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}