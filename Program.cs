namespace DiscordBotClient
{
    class Program
    {
        static void Main(string[] args) => new Bot().Start().GetAwaiter().GetResult();
    }
}