using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class Banner
    {
        
        public int Id { get; set; }
		[MaxLength(128)]
		public string ImagePath { get; set; } = string.Empty;
        public int Slot { get; set; } = 1;
        public bool Active { get; set; } = true;
    }
}
