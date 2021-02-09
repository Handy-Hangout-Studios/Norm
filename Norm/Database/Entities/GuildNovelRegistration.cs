namespace Norm.Database.Entities
{
    public class GuildNovelRegistration
    {
        public ulong GuildId { get; set; }
        public ulong AnnouncementChannelId { get; set; }
        public bool PingEveryone { get; set; }
        public bool PingNoOne { get; set; }
        public ulong? MemberId { get; set; }
        public bool IsDm { get; set; }
        public ulong? RoleId { get; set; }
        public int NovelInfoId { get; set; }

        public NovelInfo NovelInfo { get; set; }
    }
}
