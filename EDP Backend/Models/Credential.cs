using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class Credential
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public byte[] CredentialId { get; set; } = new byte[0];
        public string PublicKey { get; set; } = string.Empty;
    }
}
