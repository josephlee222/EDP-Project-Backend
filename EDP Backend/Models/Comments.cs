namespace EDP_Backend.Models
{
    public class Comments
    {
        public int Id { get; set; }
        public int PostID { get; set; }
        public int SenderID { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
