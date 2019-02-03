using System.Collections.Generic;

namespace DiscordBotClient.Quotes
{
    public interface IQuotesManager
    {
        List<Quote> Quotes { get; }

        void AddQuote(Quote quote);
        bool DeleteQuote(int ID);
        Quote GetQuote(int ID);
        Quote GetRandomQuote();
        void LoadQuotes(string file);
        Quote NewQuote();
    }
}