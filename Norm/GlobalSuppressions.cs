// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1805:Do not initialize unnecessarily", Justification = "Relying on any default value other than null is a bad idea", Scope = "member", Target = "~F:Norm.Services.ChapterUpdateBucket.populated")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "These can't be marked as static as DSharpPlus expects these to be member methods", Scope = "member", Target = "~M:Norm.Modules.GeneralModule.Tutorial(DSharpPlus.CommandsNext.CommandContext)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "These can't be marked as static as DSharpPlus expects these to be member methods", Scope = "member", Target = "~M:Norm.Modules.GeneralModule.InviteAsync(DSharpPlus.CommandsNext.CommandContext)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "While it may not be necessary to keep this DSharpPlus produces this dictionary and I suspect it's important", Scope = "member", Target = "~F:Norm.Services.BotService.interactivityDict")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "This may get added back in at some point. Needs more focus group testing", Scope = "member", Target = "~M:Norm.Services.BotService.CheckForDate(DSharpPlus.DiscordClient,DSharpPlus.EventArgs.MessageCreateEventArgs)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "This function will have the wrong signature if I get rid of this particular parameter", Scope = "member", Target = "~M:Norm.Services.BotService.CheckForDate(DSharpPlus.DiscordClient,DSharpPlus.EventArgs.MessageCreateEventArgs)~System.Threading.Tasks.Task")]
