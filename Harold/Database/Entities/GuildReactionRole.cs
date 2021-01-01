using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harold.Database.Entities
{
    public class GuildReactionRole
    {
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
        public ulong MessageId { get; set; }
    }
}
