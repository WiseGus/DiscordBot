using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using DSharpPlus;

namespace DiscordBotClient
{
    public sealed class BotConfig
    {
        public enum QuoteListType { Plain, Embed }

        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string Prefix { get; set; }

        [JsonProperty("developers")]
        public ulong[] Developers { get; set; }

        [JsonProperty("botName")]
        public string BotName { get; set; }

        [JsonProperty("avatar")]
        public string BotAvatar { get; set; }

        [JsonProperty("loglevel")]
        public LogLevel LogLevel { get; set; }

        [JsonProperty("channels")]
        public string[] Channels { get; set; }

        [JsonProperty("quoteList")]
        public QuoteListType QuoteList { get; set; }

        [JsonProperty("botGamesList")]
        public string[] BotGamesList { get; set; }

        private static BotConfig instance;

        public BotConfig() { }

        public static BotConfig Instance
        {
            get
            {
                if (instance == null)
                    CreateInstance();
                return instance;
            }
        }

        private static void CreateInstance()
        {
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            path = Path.Combine(path, "BotConfig.json");

            if (!File.Exists(path))
                throw new FileNotFoundException($"'{path}' not found.");

            var json = string.Empty;
            using (var fileStream = File.OpenRead(path))
            using (var streamReader = new StreamReader(fileStream))
                json = streamReader.ReadToEnd();
            instance = JsonConvert.DeserializeObject<BotConfig>(json);
        }

    }
}
