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
                    if (failedChecks.Any(x => x is CooldownAttribute))
                    {
                        CooldownAttribute cooldown = failedChecks.First(x => x is CooldownAttribute) as CooldownAttribute;
                        return $"this command is on cooldown for {cooldown.GetRemainingCooldown(e.Context):hh\\:mm\\:ss}";
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

                    return "The check failed is unknown";
                }

                await e.Context.RespondAsync($"You can't use `{e.Command.QualifiedName}` because {DetermineMessage()}.");
                e.Handled = true;
            }
        }

        private async Task CheckCommandExistsError(CommandsNextExtension c, CommandErrorEventArgs e)
        {
            if (e.Exception is CommandNotFoundException)
            {
                await e.Context.RespondAsync("The given command doesn't exist");
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
                    commandErrorEmbed.AddField("Message", e.Exception.Message);
                }

                if (e.Exception.StackTrace != null)
                {
                    int stackTraceLength = e.Exception?.StackTrace.Length > 1024 ? 1024 : e.Exception.StackTrace.Length;
                    commandErrorEmbed.AddField("StackTrace", e.Exception.StackTrace.Substring(0, stackTraceLength));
                }

                if (e.Exception.GetType() != null)
                {
                    commandErrorEmbed.AddField("ExceptionType", e.Exception.GetType().FullName);
                }

                await this.BotDeveloper.SendMessageAsync(embed: commandErrorEmbed);
                this.Logger.LogError(e.Exception, "Exception from Command Errored");
            }
            catch (Exception exception)
            {
                this.Logger.LogError(exception, "An error occurred in sending the exception to the Dev");
            }
        }
    }
}
