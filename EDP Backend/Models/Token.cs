using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class Token
    {
        public int Id { get; set; }
        [MaxLength(128)]
        public string Code { get; set; } = Helper.Helper.RandomString(128);
        public User User { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public DateTime Expiry { get; set; } = DateTime.Now + TimeSpan.FromMinutes(30);
    }
}
