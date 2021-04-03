namespace Norm.Configuration
{
    public class BotOptions
    {
        public static readonly string Section = "BotConfig";

        // For bot login
        public string BotToken { get; set; }

        // For logging purposes
        public ulong DevId { get; set; }
        public ulong DevGuildId { get; set; }

        // Commands Next Config Info
        public bool EnableDms { get; set; }

        public bool EnableMentionPrefix { get; set; }

        public bool EnablePrefixResolver { get; set; }

        public string[] Prefixes { get; set; }

        public DatabaseConfig Database { get; set; }

        public ulong MovieEmojiGuildId { get; set; }
        public string MovieFilePath { get; set; }
    }
}
