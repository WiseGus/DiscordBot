using DiscordBotClient.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DiscordBotClient
{
    public class Bot
    {
        private BotConfig _botConfig { get; set; }
        private DiscordClient _client { get; set; }

        public async Task Start()
        {
            Initialize();
            SetUpEvents();
            InstallCommands();

            _client.DebugLogger.LogMessage(LogLevel.Info, _botConfig.BotName, "Connecting", DateTime.Now);

            _client.GuildAvailable += e =>
            {
                _client.DebugLogger.LogMessage(LogLevel.Info, _botConfig.BotName, $"Guild available: {e.Guild.Name}", DateTime.Now);
                return Task.Delay(0);
            };

            await Connect();
        }

        private void Initialize()
        {
            _botConfig = BotConfig.Instance;

            _client = new DiscordClient(new DiscordConfiguration
            {
                AutoReconnect = true,
                LargeThreshold = 250,
                LogLevel = _botConfig.LogLevel,
                Token = _botConfig.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = false
            });
            _client.DebugLogger.LogMessageReceived += LogMessageReceived;
            _client.DebugLogger.LogMessage(LogLevel.Info, _botConfig.BotName, "Bot Initialized", DateTime.Now);
        }

        private void LogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            ConsoleWrite($"[{e.Timestamp}]", ConsoleColor.Gray);

            var color = ConsoleColor.Gray;
            if (e.Application == _botConfig.BotName)
            {
                color = ConsoleColor.White;
            }
            ConsoleWrite($"[{ e.Application}]", color);

            switch (e.Level)
            {
                case LogLevel.Info:
                    color = ConsoleColor.Green;
                    break;
                case LogLevel.Warning:
                    color = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    color = ConsoleColor.Red;
                    break;
                case LogLevel.Critical:
                    color = ConsoleColor.DarkRed;
                    break;
            }
            ConsoleWrite($"[{e.Level}]", color);
            ConsoleWrite($"{e.Message}", ConsoleColor.Gray, true);
        }

        private void ConsoleWrite(string message, ConsoleColor color, bool line = false)
        {
            Console.ForegroundColor = color;
            if (line)
            {
                Console.WriteLine(message + " ");
            }
            else
            {
                Console.Write(message + " ");
            }
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void SetUpEvents()
        {
            _client.DebugLogger.LogMessage(LogLevel.Info, _botConfig.BotName, "Setting up events", DateTime.Now);
            _client.Ready += async (ReadyEventArgs e) =>
            {
                await _client.UpdateStatusAsync(GetRandomGameName(), UserStatus.DoNotDisturb);
                await _client.EditCurrentUserAsync(_botConfig.BotName);
                var fs = new FileStream(_botConfig.BotAvatar, FileMode.Open);
                fs.Position = 0;
                try
                {
                    await _client.EditCurrentUserAsync(avatar: fs);
                }
                catch { } // Bad request on misuse?

                _client.DebugLogger.LogMessage(LogLevel.Info, _botConfig.BotName, "Ready", DateTime.Now);
                _client.DebugLogger.LogMessage(LogLevel.Info, _botConfig.BotName, $"Current user is '{_client.CurrentUser.Username}' which is connected to {_client.Guilds.Count} Guild(s).", DateTime.Now);
            };
        }

        private DiscordGame GetRandomGameName()
        {
            string[] games = _botConfig.BotGamesList;
            return games.Length > 0 ? new DiscordGame(games[new Random().Next(games.Length)]) : new DiscordGame();
        }

        private void InstallCommands()
        {
            var commands = _client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = _botConfig.Prefix,
                EnableDms = true,
                EnableMentionPrefix = true,
                EnableDefaultHelp = true
            });

            commands.CommandErrored += (e) =>
            {
                if (e.Exception is CommandNotFoundException) return Task.Delay(0);

                _client.DebugLogger.LogMessage(LogLevel.Error, _botConfig.BotName, $"CommandsNext Exception:  { e.Exception.GetType()}: { e.Exception.Message} ", DateTime.Now);
                return Task.Delay(0);
            };

            commands.CommandExecuted += (e) =>
            {
                _client.DebugLogger.LogMessage(LogLevel.Info, "CommandsNext", $"{e.Context.User.Username} executed {e.Command.Name} in {e.Context.Channel.Name}", DateTime.Now);
                return Task.Delay(0);
            };

            commands.RegisterCommands<QuoteCommands>();
            commands.RegisterCommands<AutoCommands>();
            commands.RegisterCommands<AdminCommands>();
        }

        private Task _client_MessageCreated(MessageCreateEventArgs e)
        {

            return Task.Delay(-1);
        }

        private async Task Connect()
        {
            try
            {
                await _client.ConnectAsync();
            }
            catch (Exception exc)
            {
                _client.DebugLogger.LogMessage(LogLevel.Error, _botConfig.BotName, exc.ToString(), DateTime.Now);
            }
            await Task.Delay(-1);
        }

    }

}