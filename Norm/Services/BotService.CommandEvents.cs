using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Norm.Modules;
using Norm.Modules.Exceptions;
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
            if (e.Exception is not ChecksFailedException checksFailed)
            {
                return;
            }

            IReadOnlyList<CheckBaseAttribute> failedChecks = checksFailed.FailedChecks;

            if (!failedChecks.Any())
                return;

            await e.Context.RespondAsync($"You can't use `{e.Command.QualifiedName}` because of the following reason(s):\n - {DetermineMessage(failedChecks, e.Context)}.");
            e.Handled = true;
        }

        private static string DetermineMessage(IReadOnlyList<CheckBaseAttribute> failedChecks, CommandContext context)
        {
            return string.Join("\n - ", failedChecks.Select(fc => GetFailedCheckMessage(fc, context)).Distinct());
        }

        private static string? GetFailedCheckMessage(CheckBaseAttribute failedCheck, CommandContext context) =>
            failedCheck switch
            {
                RequireBotPermissionsAttribute => "I don't have the permissions necessary",
                RequireUserPermissionsAttribute => "You don't have the permissions necessary",
                RequirePermissionsAttribute => "Either you or I don't have the permissions necessary",
                CooldownAttribute cooldownAttribute => $"This command is on cooldown for {cooldownAttribute.GetRemainingCooldown(context):hh\\:mm\\:ss}",
                RequireOwnerAttribute => "This command can only be used by the Bot's owner",
                RequireGuildAttribute => "This command must be used in a server that I'm also in",
                RequireDirectMessageAttribute => "This command must be used in a DM with me",
                _ => "An unknown failed check. To find out more please contact your bot developer."
            };

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

        public async Task CheckForFailExceptions(CommandsNextExtension c, CommandErrorEventArgs e)
        {
            string? exceptionMessage = GetExceptionMessage(e.Exception);
            if (exceptionMessage != null)
            {
                await e.Context.RespondAsync(exceptionMessage);
                e.Handled = true;
            }
        }

        private static string? GetExceptionMessage(Exception exception) =>
            exception switch
            {
                TimezoneNotSetupException => $"You do not currently have your timezone set up. This command requires your timezone in order to work. Please run `time init` to begin the process of setting up your timezone.",
                UserTimeoutException ute => $"You ran out of time during {ute.Context}. The command has been cancelled.",
                _ => null
            };

        public async Task LogCommandExceptions(CommandsNextExtension c, CommandErrorEventArgs e)
        {
            await this.LogExceptions(e.Exception);
        }

        public async Task LogExceptions(Exception exception)
        {
            try
            {
                DiscordEmbedBuilder commandErrorEmbed = new DiscordEmbedBuilder()
                    .WithTitle("Command Exception");

                if (exception.Message != null)
                {
                    AddSanitizedAndShortenedField(commandErrorEmbed, "Message", exception.Message);
                }

                if (exception.StackTrace != null)
                {
                    AddSanitizedAndShortenedField(commandErrorEmbed, "StackTrace", exception.StackTrace);
                }

                if (exception is DSharpPlus.Exceptions.UnauthorizedException u && u.JsonMessage is not null)
                {
                    AddSanitizedAndShortenedField(commandErrorEmbed, "JsonMessage", u.JsonMessage);
                }

                if (exception is DSharpPlus.Exceptions.BadRequestException b)
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

                if (exception.GetType() != null)
                {
                    AddSanitizedAndShortenedField(commandErrorEmbed, "ExceptionType", exception.GetType().FullName!);
                }

                this.Logger.LogError(exception, "Exception from Command Errored");
                await this.BotDeveloper!.SendMessageAsync(embed: commandErrorEmbed);
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "An error occurred in sending the exception to the Dev");
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
