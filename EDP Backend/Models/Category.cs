using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class Category
    {
        [MaxLength(128)]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
