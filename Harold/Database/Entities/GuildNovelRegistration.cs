namespace Harold.Database.Entities
{
    public class GuildNovelRegistration
    {
        public ulong GuildId { get; set; }
        public ulong AnnouncementChannelId { get; set; }
        public ulong? RoleId { get; set; }
        public int NovelInfoId { get; set; }
    }
}
