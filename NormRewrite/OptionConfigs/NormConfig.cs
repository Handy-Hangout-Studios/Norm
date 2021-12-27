namespace NormRewrite.OptionConfigs;

public class NormConfig
{
    public string Token { get; set; } = default!;
    public ulong DevGuildId { get; set; } = default!;

    public string MovieEmojiString { get; set; } = default!;
    public ulong MovieEmojiGuildId { get; set; } = default!;
    public string SupportServerLink { get; set; } = default!;
}