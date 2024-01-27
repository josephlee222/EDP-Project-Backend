namespace EDP_Backend.Models
{
    public class Posts
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public string Duration { get; set; } = string.Empty;
        public string LocationID { get; set; } = string.Empty;
        public int CreatorID { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
