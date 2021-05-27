using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Norm.Services
{
    public partial class BotService
    {
        private static async Task ChecksFailedError(CommandsNextExtension c, CommandErrorEventArgs e)
        {
            if (e.Exception is ChecksFailedException checksFailed)
            {
                IReadOnlyList<CheckBaseAttribute> failedChecks = checksFailed.FailedChecks;

                string DetermineMessage()
                {
                    if (failedChecks.Any(x => x is RequireBotPermissionsAttribute))
                    {
                        return "I don't have the permissions necessary";
                    }
                    if (failedChecks.Any(x => x is RequireUserPermissionsAttribute))
                    {
                        return "you don't have the permissions necessary";
                    }
                    if (failedChecks.Any(x => x is RequirePermissionsAttribute))
                    {
                        return "either you or I don't have the permissions necessary";
                    }
                    if (failedChecks.Any(x => x is CooldownAttribute))
                    {
                        CheckBaseAttribute cooldown = failedChecks.FirstOrDefault(x => x is CooldownAttribute)!;

                        return $"this command is on cooldown for {(cooldown as CooldownAttribute)!.GetRemainingCooldown(e.Context):hh\\:mm\\:ss}";
                    }
                    if (failedChecks.Any(x => x is RequireOwnerAttribute))
                    {
                        return "this command can only be used by the Bot's owner";
                    }
                    if (failedChecks.Any(x => x is RequireGuildAttribute))
                    {
                        return "this command must be used in a server that I'm also in";
                    }
                    if (failedChecks.Any(x => x is RequireDirectMessageAttribute))
                    {
                        return "this command must be used in the direct messages";
                    }

                    return "of an unknown failed check";
                }

                await e.Context.RespondAsync($"You can't use `{e.Command.QualifiedName}` because {DetermineMessage()}.");
                e.Handled = true;
            }
        }

        private async Task CheckCommandExistsError(CommandsNextExtension c, CommandErrorEventArgs e)
        {
            if (e.Exception is CommandNotFoundException)
            {
                DiscordMessage msg = await e.Context.RespondAsync("The given command doesn't exist");
                _ = Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    await msg.DeleteAsync();
                });
                e.Handled = true;
            }
            else if (e.Exception is InvalidOperationException invalid)
            {
                await e.Context.RespondAsync(invalid.Message);
                e.Handled = true;
            }
            else if (e.Exception is ArgumentException)
            {
                await e.Context.RespondAsync($"Missing or invalid arguments. Call `help {e.Command.QualifiedName}` for the proper usage.");
                e.Handled = true;
            }
        }

        private async Task LogExceptions(CommandsNextExtension c, CommandErrorEventArgs e)
        {
            try
            {
                DiscordEmbedBuilder commandErrorEmbed = new DiscordEmbedBuilder()
                    .WithTitle("Command Exception");

                if (e.Exception.Message != null)
                {
                    AddSanitizedAndShortenedField(commandErrorEmbed, "Message", e.Exception.Message);
                }

                if (e.Exception.StackTrace != null)
                {
                    AddSanitizedAndShortenedField(commandErrorEmbed, "StackTrace", e.Exception.StackTrace);
                }

                if (e.Exception is DSharpPlus.Exceptions.UnauthorizedException u && u.JsonMessage is not null)
                {
                    AddSanitizedAndShortenedField(commandErrorEmbed, "JsonMessage", u.JsonMessage);
                }

                if (e.Exception is DSharpPlus.Exceptions.BadRequestException b)
                {
                    commandErrorEmbed.AddField("Bad Request Code", b.Code.ToString(), true);
                    if (b.JsonMessage is not null)
                    {
                        AddSanitizedAndShortenedField(commandErrorEmbed, "JsonMessage", b.JsonMessage);
                    }
                    if (b.Errors is not null)
                    {
                        AddSanitizedAndShortenedField(commandErrorEmbed, "Errors", b.Errors);
                    }
                }

                if (e.Exception.GetType() != null)
                {
                    AddSanitizedAndShortenedField(commandErrorEmbed, "ExceptionType", e.Exception.GetType().FullName!);
                }

                this.Logger.LogError(e.Exception, "Exception from Command Errored");
                await this.BotDeveloper.SendMessageAsync(embed: commandErrorEmbed);
            }
            catch (Exception exception)
            {
                this.Logger.LogError(exception, "An error occurred in sending the exception to the Dev");
            }
        }

        private static void AddSanitizedAndShortenedField(DiscordEmbedBuilder embed, string title, string value)
        {
            string sanitized = Formatter.Sanitize(value);
            int sanitizedLength = sanitized.Length > 1024 ? 1024 : sanitized.Length;
            embed.AddField(title, sanitized.Substring(0, sanitizedLength));
        }
    }
}
