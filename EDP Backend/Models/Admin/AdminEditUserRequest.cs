using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class AdminEditUserRequest
    {
        [MaxLength(128), EmailAddress]
        public string Email { get; set; } = string.Empty;
        [MaxLength(128)]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*[0-9]).{8,}$", ErrorMessage = "Requires at least one letter and number")]
        public string NewPassword { get; set; } = string.Empty;
        [MaxLength(128)]
        public string? Name { get; set; }
        [MaxLength(8)]
        public string? PhoneNumber { get; set; }
        [MaxLength(64)]
        public string? OccupationalStatus { get; set; }
        [MaxLength(6)]
        public string? PostalCode { get; set; }
        [MaxLength(256)]
        public string? Address { get; set; }
        public bool? Newsletter { get; set; }
        [MaxLength(24)]
        public string? ProfilePictureType { get; set; }
        public bool? IsAdmin { get; set; } = false;

    }
}
