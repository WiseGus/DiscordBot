using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBotClient.Quotes
{
    public class QuotesUtil
    {

        public bool MessageIsValid(CommandContext ctx, string plainMessage)
        {
            return ctx.Message.Attachments.Count > 0 ||
                ctx.Message.Embeds.Count > 0 ||
                !string.IsNullOrEmpty(plainMessage);
        }

        public bool GuildChannelFilterIsValid(CommandContext ctx)
        {
            var validChannels = BotConfig.Instance.Channels;
            if (validChannels == null || validChannels.Length == 0) return true;

            var currentChannel = ctx.Guild.Name + "." + ctx.Channel.Name;
            return validChannels.FirstOrDefault(p => p.Equals(currentChannel, StringComparison.InvariantCultureIgnoreCase)) != null;
        }

        public IEnumerable<List<T>> SplitList<T>(List<T> list, int size = 25)
        {
            for (int i = 0; i < list.Count; i += size)
            {
                yield return list.GetRange(i, Math.Min(size, list.Count - i));
            }
        }
    }
}
