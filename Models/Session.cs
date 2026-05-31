namespace WebApi.Models
{
    public class Session : AbstractModel<Session>
    {
        public long UserId { get; set; }
        public required string TokenHash { get; set; }  
        public required string Device {  get; set; }
        public required string IP { get; set; }
        public DateTime ExpiredAt { get; set; }

        public User User { get; set; } = null!;
    }
}
