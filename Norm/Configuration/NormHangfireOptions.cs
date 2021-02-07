using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Configuration
{
    public class NormHangfireOptions
    {
        public static readonly string Section = "HangfireConfig";
        public DatabaseConfig Database { get; set; }
    }
}
