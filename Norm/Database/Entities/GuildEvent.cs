using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Norm.Database.Entities
{
    public class GuildEvent
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public string EventName { get; set; }
        public string EventDesc { get; set; }
    }
}