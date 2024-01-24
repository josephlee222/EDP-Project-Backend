namespace EDP_Backend.Models
{
    public class GroupMesssage
    {
        public int Id { get; set; }
        public int group_id { get; set; }
        public string sender { get; set; }
        public string message { get; set;}
        public DateTime sent_on { get; set; }
    }
}
