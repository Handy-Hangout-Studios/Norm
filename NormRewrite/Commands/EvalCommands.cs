using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace NormRewrite.Commands;

public class EvalCommands: BaseCommandModule
{
    // This Command was yoinked from Emzi0767#1837 and VelvetThePanda.
    [Command("csharp")]
    [Priority(0)]
    [RequireOwner]
    public async Task EvalCS(CommandContext ctx, [RemainingText] string code)
    {
        DiscordMessage msg;

        int cs1 = code.IndexOf("```", StringComparison.Ordinal) + 3;
        cs1 = code.IndexOf('\n', cs1) + 1;
        int cs2 = code.LastIndexOf("```", StringComparison.Ordinal);

        if (cs1 is -1 || cs2 is -1)
        {
            cs1 = 0;
            cs2 = code.Length;
        }

        string cs = code[cs1..cs2];

        msg = await ctx.RespondAsync("", new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#FF007F"))
                .WithDescription("Evaluating...")
                .Build())
            .ConfigureAwait(false);

        try
        {
            TestVariables globals = new(ctx.Message, ctx.Client, ctx);

            ScriptOptions sopts = ScriptOptions.Default;
            // TODO: Rename NormRewrite to Norm after Norm has been deprecated
            sopts = sopts.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text",
                "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Entities", "DSharpPlus.VoiceNext", "NormRewrite",
                "DSharpPlus.CommandsNext", "DSharpPlus.Interactivity",
                "Microsoft.Extensions.Logging");
            IEnumerable<System.Reflection.Assembly> asm = AppDomain.CurrentDomain.GetAssemblies()
                .Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location));
            asm = asm.Append(typeof(VoiceNextConnection).Assembly);

            sopts = sopts.WithReferences(asm);

            Script<object> script = CSharpScript.Create(cs, sopts, typeof(TestVariables));
            script.Compile();
            ScriptState<object> result = await script.RunAsync(globals).ConfigureAwait(false);
            if (result?.ReturnValue is not null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
            {
                await msg.ModifyAsync(new DiscordEmbedBuilder
                {
                    Title = "Evaluation Result",
                    Description = result.ReturnValue.ToString(),
                    Color = new DiscordColor("#007FFF")
                }.Build())
                    .ConfigureAwait(false);
            }
            else
            {
                await msg.ModifyAsync(new DiscordEmbedBuilder
                {
                    Title = "Evaluation Successful",
                    Description = "No result was returned.",
                    Color = new DiscordColor("#007FFF")
                }.Build())
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            await msg.ModifyAsync(new DiscordEmbedBuilder
            {
                Title = "Evaluation Failure",
                Description = $"**{ex.GetType()}**: {ex.Message}\n{Formatter.Sanitize(ex.StackTrace)}",
                Color = new DiscordColor("#FF0000")
            }.Build())
                .ConfigureAwait(false);
        }

    }

    // This record was yoinked from Emzi0767#1837 and VelvetThePanda.
    public record TestVariables
    {
        public DiscordMessage Message { get; }
        public DiscordChannel Channel { get; }
        public DiscordGuild? Guild { get; }
        public DiscordUser User { get; }
        public DiscordMember? Member { get; }
        public CommandContext Context { get; }

        public DiscordClient Client { get; }

        public TestVariables(DiscordMessage msg, DiscordClient client, CommandContext ctx)
        {
            this.Client = client;
            this.Context = ctx;
            this.Message = msg;
            this.Channel = msg.Channel;
            this.Guild = this.Channel.Guild;
            this.User = this.Message.Author;

            if (this.Guild != null)
            {
                this.Member = this.Guild.GetMemberAsync(this.User.Id).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
    }
}