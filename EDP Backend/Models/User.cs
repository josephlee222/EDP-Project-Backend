using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        [MaxLength(64)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(128), EmailAddress]
        public string Email { get; set; } = string.Empty;
        [MaxLength(128)]
        public string? Password { get; set; }
        [MaxLength(128)]
        public string? Passkey { get; set; }
        [MaxLength(128)]
        public string? ProfilePicture { get; set; }
        [MaxLength(8)]
        public string? PhoneNumber { get; set; } = string.Empty;
        [MaxLength(24)]
        public string? ProfilePictureType { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Balance { get; set; } = 0;
        [MaxLength(64)]
        public string? OccupationalStatus { get; set; }
        [MaxLength(6)]
        public string? PostalCode { get; set; }
        [MaxLength(256)]
        public string? Address { get; set; }
        [MaxLength(64)]
        public string? GoogleId { get; set; }
        [MaxLength(64)]
        public string? FacebookId { get; set; }
        public Boolean Newsletter { get; set; } = false;
        public Boolean IsAdmin { get; set; } = false;
        public Boolean IsDeleted { get; set; } = false;
        public Boolean IsVerified { get; set; } = false;
        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column(TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Cart> Cart { get; set; } = new List<Cart>();
    }
}