using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Options;
using NormRewrite.OptionConfigs;

namespace NormRewrite.SlashCommands;

public class FunModule: ApplicationCommandModule
{
    private readonly NormConfig _normConfig;

    public FunModule(IOptions<NormConfig> options)
    {
        this._normConfig = options.Value;
    }

    [SlashCommand("movie", "Plays a movie using emojis")]
    public async Task OutputMovie(InteractionContext context)
    {
        string pattern = @":(?<name>.*?):";
        DiscordGuild movieGuild = context.Client.Guilds[this._normConfig.MovieEmojiGuildId];
        Dictionary<string, DiscordEmoji> emojiMap = movieGuild.Emojis.Select(kvp => kvp.Value)
            .ToDictionary(e => e.Name);
        string outputString = Regex.Replace(this._normConfig.MovieEmojiString, pattern,
            m => emojiMap[m.Groups["name"].Value].ToString());

        DiscordInteractionResponseBuilder response = new DiscordInteractionResponseBuilder()
            .WithContent(outputString);

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
    }
}