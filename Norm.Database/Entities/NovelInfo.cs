using System.Collections.Generic;

namespace Norm.Database.Entities
{
    public class NovelInfo
    {
        public int Id { get; set; }
        public ulong FictionId { get; set; }
        public string Name { get; set; }
        public string SyndicationUri { get; set; }
        public string FictionUri { get; set; }
        public ulong MostRecentChapterId { get; set; }

        public ICollection<GuildNovelRegistration> AssociatedGuildNovelRegistrations { get; set; }
    }
}
