﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace ColorBot
{
    public class Bot
    {
        public Bot(Settings settings)
        {
            CurrentSettings = settings;
        }

        private Settings CurrentSettings { get; }

        public async Task Initialize()
        {
            var discord = new DiscordClient(new DiscordConfiguration
            {
                Token = CurrentSettings.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] {"/"},
                EnableDefaultHelp = false,
                IgnoreExtraArguments = true,
                EnableDms = false,
                EnableMentionPrefix = false
            });

            commands.RegisterCommands<ColorCommands>();
            commands.RegisterCommands<ModCommands>();


            discord.ClientErrored += Client_ClientError;
            commands.CommandExecuted += Commands_CommandExecuted;
            commands.CommandErrored += Commands_CommandErrored;

            ColorCommands.CurrentSettings = CurrentSettings;
            ModCommands.CurrentSettings = CurrentSettings;
            ColorRegistry.CurrentSettings = CurrentSettings;


            await discord.ConnectAsync();
        }

        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            // let's log the details of the error that just 
            // occured in our client
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "ExampleBot",
                $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            // let's log the name of the command and user
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot",
                $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            // let's log the error details
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "ExampleBot",
                $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}",
                DateTime.Now);
            
            var emoji = ":warning:";
            var message = $"An error occurred executing {e.Command?.Name}";
            if (e.Exception is ChecksFailedException cfe)
            {
                var check = cfe.FailedChecks.FirstOrDefault();
                if (check is RequireUserPermissionsAttribute rupe)
                {
                    emoji = ":no_entry:";
                    message = $"You do not have the permissions [{rupe.Permissions}] which are required to execute this command.";
                }
                else
                {
                    message = $"Check failed: {check?.GetType().Name}";
                }
            }

            // let's wrap the response into an embed
            var embed = new DiscordEmbedBuilder
            {
                Title = "Error",
                Description = $"{DiscordEmoji.FromName(e.Context.Client, emoji)} {message}",
                Color = new DiscordColor(0xFF0000) // red
            };
            await e.Context.RespondAsync("", embed: embed);
        }
    }
}