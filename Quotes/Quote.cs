namespace DiscordBotClient.Quotes
{
    public class Quote
    {
        public enum StorageDataType { PlainMessage, EmbedMessage, Image }

        public int ID { get; set; }
        public StorageDataType Type { get; set; }
        public object Data { get; set; }
    }
}
