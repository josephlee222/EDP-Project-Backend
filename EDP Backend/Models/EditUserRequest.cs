using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class EditUserRequest
    {
        [MaxLength(128)]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*[0-9]).{8,}$", ErrorMessage = "Requires at least one letter and number")]
        public string? Password { get; set; }
        [MaxLength(128)]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*[0-9]).{8,}$", ErrorMessage = "Requires at least one letter and number")]
        public string? NewPassword { get; set; }
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
        public Boolean? Newsletter { get; set; }
        [MaxLength(24)]
        public string? ProfilePictureType { get; set; }
        public string? ProfilePicture { get; set; }
    }
}
