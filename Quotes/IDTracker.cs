namespace DiscordBotClient.Quotes
{
    internal static class IDTracker
    {
        private static int _CurrentID = 0;

        public static int NewID()
        {
            _CurrentID++;
            return _CurrentID;
        }

        public static void SetID(int ID)
        {
            _CurrentID = ID;
        }
    }
}
