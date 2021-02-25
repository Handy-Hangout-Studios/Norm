namespace Norm.Configuration
{
    public class NormHangfireOptions
    {
        public static readonly string Section = "HangfireConfig";
        public DatabaseConfig Database { get; set; }
    }
}
