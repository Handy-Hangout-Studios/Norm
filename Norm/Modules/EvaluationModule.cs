using CSharpMath.SkiaSharp;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Norm.Attributes;
using SkiaSharp;
using System.Drawing;
using System.Collections.Generic;
using CSharpMath.Rendering.BackEnd;
using Typography.OpenFont;

namespace Norm.Modules
{


    // This Command was yoinked from Emzi0767#1837 and VelvetThePanda.
    public class EvaluationModule : BaseCommandModule
    {
        [Command("tex")]
        [Description("Produce a PNG of the LaTeX code in the code block. Note that the LaTeX must be in a code block like so\n\\`\\`\\`\nLaTeX here\n\\`\\`\\`")]
        [BotCategory(BotCategory.Evaluation)]
        public async Task EvaluateTex(CommandContext context, [RemainingText][Description("The LaTeX code in a code block to render as an image")] string latex)
        {
            int upperBound = latex.IndexOf("```", StringComparison.Ordinal) + 3;
            upperBound = latex.IndexOf('\n', upperBound) + 1;
            int lowerBound = latex.LastIndexOf("```", StringComparison.Ordinal);

            bool lightMode = latex.Contains("--light");

            if (upperBound is -1 || lowerBound is -1)
            {
                upperBound = 0;
                lowerBound = latex.Length;
            }

            using Stream fontStream = new FileStream("IBMPlexSans-Text.otf", FileMode.Open);

            TextPainter painter = new TextPainter { 
                LaTeX = $"{latex[upperBound..lowerBound]}", 
                FontSize = 20, 
                TextColor = lightMode ? SKColors.Black : SKColors.White,
            };

            RectangleF bounds = painter.Measure(800);

            using SKSurface surface = SKSurface.Create(new SKImageInfo((int)Math.Ceiling(bounds.Width)+50, (int)Math.Ceiling(bounds.Height)+50));
            using SKCanvas canvas = surface.Canvas;
            canvas.Clear(lightMode ? SKColors.White : SKColors.Black);
            painter.Draw(canvas, new PointF(25, 25), bounds.Width);
            SKData snapshot = surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100);

            using Stream png = snapshot.AsStream();
            DiscordMessageBuilder builder = new DiscordMessageBuilder().WithFile("latex.png", png);

            await context.RespondAsync(builder);
        }

        [Command("csharp")]
        [Description("Evaluates C# code.")]
        [RequireOwner]
        [Priority(1)]
        [BotCategory(BotCategory.Evaluation)]
        public async Task EvalCS(CommandContext ctx)
        {
            if (ctx.Message.ReferencedMessage is null) await EvalCS(ctx, ctx.RawArgumentString);
            else
            {
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
                string? code = ctx.Message.ReferencedMessage.Content;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
                if (code.Contains(ctx.Prefix))
                {
                    int index = code.IndexOf(' ');
                    code = code[++index..];
                }
                await EvalCS(ctx, code);
            }
        }


        [Command("csharp")]
        [Priority(0)]
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
                var globals = new TestVariables(ctx.Message, ctx.Client, ctx);

                var sopts = ScriptOptions.Default;
                sopts = sopts.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text",
                    "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Entities", "DSharpPlus.VoiceNext", "Norm",
                    "DSharpPlus.CommandsNext", "DSharpPlus.Interactivity",
                    "Microsoft.Extensions.Logging");
                var asm = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location));
                asm = asm.Append(typeof(VoiceNextConnection).Assembly);

                sopts = sopts.WithReferences(asm);

                Script<object> script = CSharpScript.Create(cs, sopts, typeof(TestVariables));
                script.Compile();
                ScriptState<object> result = await script.RunAsync(globals).ConfigureAwait(false);
                if (result?.ReturnValue is not null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
                    await msg.ModifyAsync(new DiscordEmbedBuilder
                    {
                        Title = "Evaluation Result",
                        Description = result.ReturnValue.ToString(),
                        Color = new DiscordColor("#007FFF")
                    }.Build())
                        .ConfigureAwait(false);
                else
                    await msg.ModifyAsync(new DiscordEmbedBuilder
                    {
                        Title = "Evaluation Successful",
                        Description = "No result was returned.",
                        Color = new DiscordColor("#007FFF")
                    }.Build())
                        .ConfigureAwait(false);
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

        public record TestVariables
        {
            public DiscordMessage Message { get; }
            public DiscordChannel Channel { get; }
            public DiscordGuild Guild { get; }
            public DiscordUser User { get; }
            public DiscordMember Member { get; }
            public CommandContext Context { get; }

            public DiscordClient Client { get; }

            public TestVariables(DiscordMessage msg, DiscordClient client, CommandContext ctx)
            {
                Client = client;
                Context = ctx;
                Message = msg;
                Channel = msg.Channel;
                Guild = Channel.Guild;
                User = Message.Author;

                if (Guild != null) Member = Guild.GetMemberAsync(User.Id).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
    }


}