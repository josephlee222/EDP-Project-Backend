using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models.Admin
    {
        public class EditActivityRequest
        {

            [MaxLength(128)]
            public string Name { get; set; } = string.Empty;
            public DateTime ExpiryDate { get; set; }
            public string? Description { get; set; } = string.Empty;
            public string? Category { get; set; } = string.Empty;
            public bool? NtucExclusive { get; set; } = false;
            public int? AgeLimit { get; set; }
            public string Location { get; set; } = string.Empty;
            public string Company { get; set; } = string.Empty;
            public string? DiscountType { get; set; }
            public float? DiscountAmount { get; set; }
            public bool? Discounted { get; set; } = false;
            //public array Pictures { get; set; }
        }
    }
