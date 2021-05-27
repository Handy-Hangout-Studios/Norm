namespace Norm.Configuration
{
#nullable disable
    public class NormHangfireOptions
    {
        public static readonly string Section = "HangfireConfig";
        public DatabaseConfig Database { get; set; }
    }
}
