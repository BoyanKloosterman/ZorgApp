namespace ZorgAppAPI.Models
{
    public class UserZorgMoment
    {
        public string UserId { get; set; }
        public int ZorgMomentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
