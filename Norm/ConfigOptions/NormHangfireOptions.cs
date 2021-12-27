namespace Norm.ConfigOptions
{
#nullable disable
    public class NormHangfireOptions
    {
        public static readonly string Section = "HangfireConfig";
        public DatabaseConfig Database { get; set; }
    }
}
