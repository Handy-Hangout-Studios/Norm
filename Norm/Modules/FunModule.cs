using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Emzi0767.Utilities;
using Microsoft.Extensions.Options;
using Norm.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Modules
{
    public class FunModule : BaseCommandModule
    {
        private readonly IOptions<BotOptions> options;
        public FunModule(IOptions<BotOptions> options)
        {
            this.options = options;
        }

        [Command("beemovie")]
        [Description("Play the bee movie using emojis and a text file")]
        public async Task OutputBeeMovie(CommandContext context)
        {
            StringBuilder contentBuilder = new();

            DiscordGuild beeMovieGuild = await context.Client.GetGuildAsync(this.options.Value.BeeMovieEmojiGuildId);
            Dictionary<string, DiscordEmoji> beeMovieEmojis = beeMovieGuild.Emojis.Values.ToList().ToDictionary(emoji => emoji.Name);

            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    contentBuilder.Append(beeMovieEmojis[$"beemovie{column}x{row}"].ToString());
                }
                contentBuilder.AppendLine();
            }

            using FileStream fs = new(this.options.Value.BeeMovieFilePath, FileMode.Open);
            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder().WithContent(contentBuilder.ToString());
            messageBuilder.WithFile(fs);
            await context.RespondAsync(messageBuilder);
        }
    }
}
