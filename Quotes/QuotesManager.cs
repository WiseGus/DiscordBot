using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DiscordBotClient.Quotes
{
    public class QuotesManager : IQuotesManager
    {
        public List<Quote> Quotes { get; private set; }

        private string _quotesFile;

        public void LoadQuotes(string file)
        {
            _quotesFile = file;
            InitQuotes();
        }

        public Quote GetRandomQuote()
        {
            if (Quotes.Count == 0) return null;

            var _rnd = new Random();
            var no = _rnd.Next(0, Quotes.Count);
            return Quotes[no];
        }

        public Quote GetQuote(int ID)
        {
            return Quotes.Find(p => p.ID == ID);
        }

        public Quote NewQuote()
        {
            return new Quote
            {
                ID = IDTracker.NewID()
            };
        }

        public void AddQuote(Quote quote)
        {
            Quotes.Add(quote);

            var quotesStr = Newtonsoft.Json.JsonConvert.SerializeObject(Quotes, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(_quotesFile, quotesStr);
        }

        public bool DeleteQuote(int ID)
        {
            bool res = false;

            var quote = GetQuote(ID);
            if (quote != null)
            {
                res = Quotes.Remove(quote);

                var quotesStr = Newtonsoft.Json.JsonConvert.SerializeObject(Quotes, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_quotesFile, quotesStr);

            }
            return res;
        }

        private void InitQuotes()
        {
            if (File.Exists(_quotesFile))
            {
                var quotesStr = File.ReadAllText(_quotesFile);
                try
                {
                    Quotes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Quote>>(quotesStr);
                    Quotes.ForEach(quote =>
                    {
                        switch (quote.Type)
                        {
                            case Quote.StorageDataType.EmbedMessage:
                                quote.Data = Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordEmbed>(Convert.ToString(quote.Data));
                                break;
                            case Quote.StorageDataType.Image:
                                quote.Data = Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordAttachment>(Convert.ToString(quote.Data));
                                break;
                        }
                    });

                    IDTracker.SetID(Quotes.Max(p => p.ID));
                }
                catch
                {
                    Quotes = new List<Quote>();
                }
            }
            else
            {
                Quotes = new List<Quote>();
            }
        }
    }
}
