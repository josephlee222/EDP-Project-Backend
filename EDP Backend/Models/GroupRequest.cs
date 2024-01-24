namespace EDP_Backend.Models
{
    public class GroupRequest
    {
        public int Id { get; set; }
        public int sender { get; set; }
        public int receiver { get; set; }
        public DateTime request_on { get; set; }
    }
}
    
