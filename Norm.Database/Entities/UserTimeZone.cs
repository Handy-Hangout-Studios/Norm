namespace Norm.Database.Entities
{
    public class UserTimeZone
    {
        public UserTimeZone(ulong userId, string timeZoneId)
        {
            this.UserId = userId;
            this.TimeZoneId = timeZoneId;
        }

        public int Id { get; set; }
        public ulong UserId { get; set; }
        public string TimeZoneId { get; set; }
    }
}