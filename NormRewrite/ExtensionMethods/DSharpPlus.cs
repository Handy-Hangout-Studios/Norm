using DSharpPlus;
using DSharpPlus.Entities;

namespace NormRewrite.ExtensionMethods;

public static class DSharpPlusExtensions
{
    public static string GenerateBotCommandOAuth(this DiscordApplication app, Permissions perms)
    {
        return app.GenerateBotOAuth(perms).Replace("&scope=", "&scope=applications.commands%20");
    }
}