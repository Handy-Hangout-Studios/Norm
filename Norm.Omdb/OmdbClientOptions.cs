using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Omdb
{
#nullable disable
    public class OmdbClientOptions
    {
        public string ApiKey { get; set; }
        public int? Version { get; set; }
    }
}
