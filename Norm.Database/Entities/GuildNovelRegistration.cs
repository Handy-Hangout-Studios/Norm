using System;
using System.Diagnostics.CodeAnalysis;

namespace Norm.Database.Entities
{
    public class GuildNovelRegistration
    {
        public GuildNovelRegistration(ulong guildId, ulong announcementChannelId, bool pingEveryone, bool pingNoOne, ulong? memberId, bool isDm, ulong? roleId, int novelInfoId)
        {
            this.GuildId = guildId;
            this.AnnouncementChannelId = announcementChannelId;
            this.PingEveryone = pingEveryone;
            this.PingNoOne = pingNoOne;
            this.MemberId = memberId;
            this.IsDm = isDm;
            this.RoleId = roleId;
            this.NovelInfoId = novelInfoId;
        }

        public ulong GuildId { get; set; }
        public ulong AnnouncementChannelId { get; set; }
        public bool PingEveryone { get; set; }
        public bool PingNoOne { get; set; }
        public ulong? MemberId { get; set; }

        [MemberNotNullWhen(true, nameof(MemberId))]
        [MemberNotNullWhen(false, nameof(AnnouncementChannelId))]
        public bool IsDm { get; set; }
        public ulong? RoleId { get; set; }
        public int NovelInfoId { get; set; }

        public NovelInfo NovelInfo { get; set; } = null!;
    }
}
