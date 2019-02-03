using DiscordBotClient.Quotes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DiscordBotClient
{
    [Group("admin")]
    [Description("Administrative commands.")]
    [Hidden]
    public class AdminCommands
    {
        [Command("ClearCommandsHistory"), Description("Clear latest n commands"), Hidden, RequireOwner]
        public async Task ClearCommandsHistory(CommandContext ctx, int number)
        {
            var messages = await ctx.Channel.GetMessagesAsync(Math.Min(number, 100));
            await ctx.Channel.DeleteMessagesAsync(messages);
        }
    }

    public class QuoteCommands
    {
        private IQuotesManager _quotesManager;
        private QuotesUtil _util;

        public QuoteCommands()
        {
            _util = new QuotesUtil();

            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Quotes.json");
            _quotesManager = new QuotesManager();
            _quotesManager.LoadQuotes(file);
        }

        [Command("list"), Description("Shows the stored list of quotes")]
        public async Task List(CommandContext ctx)
        {
            if (!_util.GuildChannelFilterIsValid(ctx)) return;

            await ctx.TriggerTypingAsync();

            switch (BotConfig.Instance.QuoteList)
            {
                case BotConfig.QuoteListType.Plain:
                    await DoListPlainAsync(ctx);
                    break;
                case BotConfig.QuoteListType.Embed:
                    await DoListEmbedAsync(ctx);
                    break;
            }
        }

        [Command("add"), Description("Add a quote/image/file/link etc..."), Aliases("a")]
        public async Task Add(CommandContext ctx, [RemainingText] string plainMessage)
        {
            if (!_util.GuildChannelFilterIsValid(ctx)) return;

            await ctx.TriggerTypingAsync();

            if (!_util.MessageIsValid(ctx, plainMessage))
            {
                await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, $":thumbsdown:"));
                return;
            }

            var quote = DoAdd(plainMessage, ctx.Message);
            var emoji = DiscordEmoji.FromName(ctx.Client, $":thumbsup:");
            await ctx.RespondAsync($"{emoji} [id:{quote.ID}]");
        }

        [Command("quote"), Description("Get a quote/image/file/link etc..."), Aliases("q", "get", "g")]
        public async Task Get(CommandContext ctx, int ID = 0)
        {
            if (!_util.GuildChannelFilterIsValid(ctx)) return;

            await ctx.TriggerTypingAsync();

            Quote Quote;
            if (ID <= 0)
            {
                Quote = _quotesManager.GetRandomQuote();
            }
            else
            {
                Quote = _quotesManager.GetQuote(ID);
            }

            if (Quote != null)
            {
                var embed = DoGet(ctx, Quote);
                if (embed != null)
                {
                    await ctx.RespondAsync(embed: embed);
                }
                else
                {
                    var emoji = DiscordEmoji.FromName(ctx.Client, ":thumbsdown:");
                    await ctx.RespondAsync($"{emoji}");
                }
            }
            else
            {
                var emoji = DiscordEmoji.FromName(ctx.Client, ":thumbsdown:");
                await ctx.RespondAsync($"{emoji}");
            }
        }

        [Command("remove"), Description("Delete a quote"), Hidden, RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
        public async Task Remove(CommandContext ctx, int ID)
        {
            if (!_util.GuildChannelFilterIsValid(ctx)) return;

            await ctx.TriggerTypingAsync();

            var res = _quotesManager.DeleteQuote(ID);
            var emoji = DiscordEmoji.FromName(ctx.Client, res ? ":thumbsup:" : ":thumbsdown:");
            await ctx.RespondAsync($"{emoji}");
        }

        private async Task DoListPlainAsync(CommandContext ctx)
        {
            var quotesSplit = _util.SplitList(_quotesManager.Quotes);
            foreach (var quoteSplit in quotesSplit)
            {
                var quotesPlain = new List<string>();
                foreach (var quote in quoteSplit)
                {
                    string value;
                    switch (quote.Type)
                    {
                        case Quote.StorageDataType.EmbedMessage:
                            value = (quote.Data as DiscordEmbed).Title;
                            break;
                        case Quote.StorageDataType.Image:
                            value = (quote.Data as DiscordAttachment).FileName;
                            break;
                        default:
                            value = Convert.ToString(quote.Data);
                            break;
                    }
                    quotesPlain.Add(quote.ID.ToString() + ": " + value);
                }
                await ctx.RespondAsync(string.Join(Environment.NewLine, quotesPlain));
            }
        }

        private async Task DoListEmbedAsync(CommandContext ctx)
        {
            var quotesSplit = _util.SplitList(_quotesManager.Quotes);
            foreach (var quoteSplit in quotesSplit)
            {
                var quotesEmbed = new DiscordEmbedBuilder
                {
                    Title = "Quotes List",
                    Color = DiscordColor.Green
                };

                foreach (var quote in quoteSplit)
                {
                    string value;
                    switch (quote.Type)
                    {
                        case Quote.StorageDataType.EmbedMessage:
                            value = (quote.Data as DiscordEmbed).Title;
                            break;
                        case Quote.StorageDataType.Image:
                            value = (quote.Data as DiscordAttachment).FileName;
                            break;
                        default:
                            value = Convert.ToString(quote.Data);
                            break;
                    }
                    quotesEmbed.AddField(quote.ID.ToString(), value);
                }

                await ctx.RespondAsync(embed: quotesEmbed);
            }
        }

        private DiscordEmbed DoGet(CommandContext ctx, Quote Quote)
        {
            switch (Quote.Type)
            {
                case Quote.StorageDataType.PlainMessage:
                    var msg = Convert.ToString(Quote.Data);

                    var author = string.Empty;
                    var msgSplit = msg.Split(new[] { "-", " - " }, StringSplitOptions.RemoveEmptyEntries);
                    if (msgSplit.Length > 1)
                    {
                        var wordsAfterLastDash = msgSplit[msgSplit.Length - 1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (wordsAfterLastDash.Length > 0 && wordsAfterLastDash.Length <= 2)
                        {
                            author = msgSplit[msgSplit.Length - 1];
                            msg = msg.Remove(msg.LastIndexOf(author)).Remove(msg.LastIndexOf('-'));
                        }
                    }

                    var res = new DiscordEmbedBuilder { Description = msg };

                    if (!string.IsNullOrEmpty(author))
                    {
                        res.Author = new DiscordEmbedBuilder.EmbedAuthor { Name = author };
                    }

                    return res;

                case Quote.StorageDataType.EmbedMessage:
                    return Quote.Data as DiscordEmbed;
                case Quote.StorageDataType.Image:
                    var attachment = Quote.Data as DiscordAttachment;
                    return new DiscordEmbedBuilder
                    {
                        Title = attachment.FileName,
                        ImageUrl = attachment.Url
                    };
            }
            return null;
        }

        private Quote DoAdd(string plainMessage, DiscordMessage advancedMessage)
        {
            var quote = _quotesManager.NewQuote();

            if (advancedMessage.Attachments.Count > 0)
            {
                quote.Type = Quote.StorageDataType.Image;
                quote.Data = advancedMessage.Attachments[0];
            }
            else if (advancedMessage.Embeds.Count > 0)
            {
                quote.Type = Quote.StorageDataType.EmbedMessage;
                quote.Data = advancedMessage.Embeds[0];
            }
            else
            {
                quote.Type = Quote.StorageDataType.PlainMessage;
                quote.Data = plainMessage;
            }

            _quotesManager.AddQuote(quote);

            return quote;
        }

    }

}