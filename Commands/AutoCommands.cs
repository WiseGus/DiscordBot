using DiscordBotClient.Quotes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBotClient.Commands
{
    class AutoCommands
    {
        private IQuotesManager _quotesManager;
        private QuotesUtil _util;
        private CancellationTokenSource _cancellationTokenSource;
        private Random _rand;
        private bool _started = false;

        public AutoCommands()
        {
            _util = new QuotesUtil();
            _rand = new Random();

            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoQuotes.json");
            _quotesManager = new QuotesManager();
            _quotesManager.LoadQuotes(file);
        }

        [Command("autostart"), Hidden, RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
        public async Task Start(CommandContext ctx, int randomMinutesFrom = 0, int randomMinutesTo = 0)
        {
            if (_quotesManager.Quotes.Count == 0) return;
            if (!_util.GuildChannelFilterIsValid(ctx)) return;
            if (_started) return;

            _started = true;
            _cancellationTokenSource = new CancellationTokenSource();

            await DoSpeakAsync(ctx, randomMinutesFrom, randomMinutesTo);
        }

        [Command("autostop"), Hidden, RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
        public async Task Stop(CommandContext ctx)
        {
            if (!_util.GuildChannelFilterIsValid(ctx)) return;
            if (!_started) return;

            _started = false;
            _cancellationTokenSource.Cancel();
            await Task.Delay(0);
        }

        private async Task DoSpeakAsync(CommandContext ctx, int randomMinutesFrom = 0, int randomMinutesTo = 0)
        {
            while (true)
            {
                Quote Quote = _quotesManager.GetRandomQuote();
                if (Quote == null) return;

                var embed = DoGet(ctx, Quote);
                if (embed != null)
                {
                    await ctx.Channel.SendMessageAsync(embed: embed);
                }

                var waitTime = SetWaitTime(randomMinutesFrom, randomMinutesTo);
                await Task.Delay(waitTime, _cancellationTokenSource.Token);
            };
        }

        private DiscordEmbed DoGet(CommandContext ctx, Quote Quote)
        {
            switch (Quote.Type)
            {
                case Quote.StorageDataType.PlainMessage:
                    return new DiscordEmbedBuilder { Description = Convert.ToString(Quote.Data) };
                case Quote.StorageDataType.EmbedMessage:
                    return Quote.Data as DiscordEmbed;
                case Quote.StorageDataType.Image:
                    var attachment = Quote.Data as DiscordAttachment;
                    return new DiscordEmbedBuilder
                    {
                        Title = attachment.FileName,
                        ImageUrl = attachment.Url,
                        Color = DiscordColor.Yellow
                    };
            }
            return null;
        }

        private TimeSpan SetWaitTime(int randomMinutesFrom, int randomMinutesTo)
        {
            var waitTime = TimeSpan.FromMinutes(5);

            if (randomMinutesFrom == 0 && randomMinutesTo == 0) return waitTime;
            if (randomMinutesFrom >= randomMinutesTo) return waitTime;

            waitTime = TimeSpan.FromMinutes(_rand.Next(randomMinutesFrom, randomMinutesTo));

            return waitTime;
        }
    }
}
