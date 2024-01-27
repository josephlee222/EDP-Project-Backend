namespace EDP_Backend.Models
{
    public class GroupRequest
    {
        public int Id { get; set; }
        public int SenderID { get; set; }
        public int RecipientID { get; set; }
        public DateTime SentAt { get; set; }
    }
}
    
