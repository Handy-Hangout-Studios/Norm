using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Emzi0767.Utilities;
using Microsoft.Extensions.Options;
using Norm.Attributes;
using Norm.Configuration;
using Owoify;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        [Command("movie")]
        [Description("Plays a movie using emojis and a text file")]
        [BotCategory(BotCategory.Miscellaneous)]
        public async Task OutputBeeMovie(CommandContext context)
        {
            StringBuilder contentBuilder = new();

            DiscordGuild movieGuild = await context.Client.GetGuildAsync(this.options.Value.MovieEmojiGuildId);
            Dictionary<string, DiscordEmoji> movieEmojis = movieGuild.Emojis.Values.ToList().ToDictionary(emoji => emoji.Name);

            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    contentBuilder.Append(movieEmojis[$"beemovie{column}x{row}"].ToString());
                }
                contentBuilder.AppendLine();
            }

            using FileStream fs = new(this.options.Value.MovieFilePath, FileMode.Open);
            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder().WithContent(contentBuilder.ToString());
            messageBuilder.WithFile("movie", fs);
            await context.RespondAsync(messageBuilder);
        }

        [Command("say")]
        [Description("Have Norm say the message as himself. If you reply to a message and use this command then Norm will also will reply to the message with the same mention settings you used.\n" + FormattingDescription)]
        public async Task SayAsync(CommandContext context, [RemainingText][Description("The message to say as well as the options you want used in the message")] string message = "")
        {
            DiscordMessageBuilder builder = new DiscordMessageBuilder().WithAllowedMentions(Mentions.None.Union(new List<IMention> { new UserMention(), }));

            if (context.Message.MessageType.HasValue && context.Message.MessageType.Value.Equals(MessageType.Reply))
            {
                DiscordMessage msg = await context.Channel.GetMessageAsync(context.Message.Id);
                builder.WithReply(msg.ReferencedMessage.Id, msg.MentionedUsers.Contains(msg.ReferencedMessage.Author));
            }

            message = await CheckAndDeleteOnHide(context, message);

            string content = ParseOptionsAndEdit(message);
            List<string> allMessages = GenerateIndividualMessages(content);

            foreach (string c in allMessages)
            {
                builder.WithContent(c);
                await context.Channel.SendMessageAsync(builder);
                builder.Clear();
            }
        }

        private static List<string> GenerateIndividualMessages(string content)
        {
            List<string> allMessages = new();
            int numBytes = 0;
            StringBuilder stringBuilder = new();
            StringBuilder wordBuilder = new();
            TextElementEnumerator te = StringInfo.GetTextElementEnumerator(content);
            while (te.MoveNext())
            {
                string teString = te.GetTextElement();
                int teNumBytes = teString.EnumerateRunes().Sum(x => x.Utf8SequenceLength);
                numBytes += teNumBytes;
                if (numBytes >= 2000)
                {
                    allMessages.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    numBytes = teNumBytes;
                }
                wordBuilder.Append(teString);
                if (string.IsNullOrWhiteSpace(teString))
                {
                    stringBuilder.Append(wordBuilder);
                    wordBuilder.Clear();
                }
            }

            allMessages.Add(stringBuilder.Append(wordBuilder).ToString());
            return allMessages;
        }

        [Command("me")]
        [RequireBotPermissions(Permissions.ManageWebhooks)]
        [Description("Say a message as yourself but with some extra formatting added.\n" + FormattingDescription)]
        [BotCategory(BotCategory.Miscellaneous)]
        public async Task SayAsAuthorAsync(CommandContext context, [RemainingText][Description("The message to say as you as well as the options you want used in the message")] string message = "")
        {
            message = await CheckAndDeleteOnHide(context, message);
            DiscordWebhook webhook = (await context.Channel.GetWebhooksAsync()).FirstOrDefault(wbhk => wbhk.Name.Equals("Norm"));
            if (webhook is null)
            {
                webhook = await context.Channel.CreateWebhookAsync("Norm");
            }

            DiscordWebhookBuilder wBuilder = new DiscordWebhookBuilder()
                .WithContent(ParseOptionsAndEdit(message))
                .WithAvatarUrl(context.Member.AvatarUrl)
                .WithUsername(context.Member.DisplayName)
                .AddMentions(Mentions.None.Union(new List<IMention> { new UserMention(), }));
            await webhook.ExecuteAsync(wBuilder);
        }


        private static async Task<string> CheckAndDeleteOnHide(CommandContext context, string message)
        {

            if (message.Contains(HideIndicator))
            {
                if (context.Guild.CurrentMember.PermissionsIn(context.Channel).HasPermission(Permissions.ManageMessages))
                {
                    await context.Message.DeleteAsync();
                }

                message = message.Replace(HideIndicator, string.Empty);
            }

            return message;
        }

        private static string ParseOptionsAndEdit(string message)
        {
            Owoifier.OwoifyLevel? owoLevel = null;
            if (message.Contains(OwOIndicator))
            {
                owoLevel = Owoifier.OwoifyLevel.Owo;
                message = message.Replace(OwOIndicator, string.Empty);
            }
            if (message.Contains(UwUIndicator))
            {
                owoLevel = Owoifier.OwoifyLevel.Uwu;
                message = message.Replace(UwUIndicator, string.Empty);
            }
            if (message.Contains(UvUIndicator))
            {
                owoLevel = Owoifier.OwoifyLevel.Uvu;
                message = message.Replace(UvUIndicator, string.Empty);
            }
            bool sarcasmify;
            if (sarcasmify = message.Contains(SarcasmifyIndicator))
            {
                message = message.Replace(SarcasmifyIndicator, string.Empty);
            }

            bool clappify;
            if (clappify = message.Contains(ClappifyIndicator))
            {
                message = message.Replace(ClappifyIndicator, string.Empty);
            }

            message = message.Trim();

            if (owoLevel.HasValue)
            {
                message = Owoifier.Owoify(message, (Owoifier.OwoifyLevel)owoLevel);
            }
            if (sarcasmify)
            {
                message = Sarcasmify(message);
            }
            if (clappify)
            {
                message = Clappify(message);
            }

            return message;
        }

        private static string Sarcasmify(string message)
        {
            StringBuilder builder = new();
            bool caps = false;
            foreach (char c in message)
            {
                if (char.IsLetter(c))
                {
                    caps = !caps;
                    builder.Append(caps ? char.ToUpper(c) : char.ToLower(c));
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        private static string Clappify(string message)
        {
            return string.Join(" 👏 ", message.Split(' '));
        }

        private const string HideIndicator = "--hide";
        private const string HideDescription = "If you add `" + HideIndicator + "` and Norm has permission to delete messages in that channel he will delete your message.";
        private const string OwOIndicator = "--owo";
        private const string UwUIndicator = "--uwu";
        private const string UvUIndicator = "--uvu";
        private const string OwOifyDescription = "If you add `" + OwOIndicator + "`, `" + UwUIndicator + "`, or `" + UvUIndicator + "` to your message then Norm will OwOify your message from readable to completely unreadable depending on which type you choose.";
        private const string SarcasmifyIndicator = "--sarcasm";
        private const string SarcasmifyDescription = "If you add `" + SarcasmifyIndicator + "` to your message then Norm will say the message alternating caps letters.";
        private const string ClappifyIndicator = "--clap";
        private const string ClappifyDescription = "If you add `" + ClappifyIndicator + "` to your message then Norm will replace every space with \" 👏 \"";
        private const string FormattingDescription = HideDescription + "\n" + OwOifyDescription + "\n" + SarcasmifyDescription + "\n" + ClappifyDescription;
    }
}
