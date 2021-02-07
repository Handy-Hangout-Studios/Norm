namespace Norm.Database.Entities
{
    public class UserTimeZone
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public string TimeZoneId { get; set; }
    }
}