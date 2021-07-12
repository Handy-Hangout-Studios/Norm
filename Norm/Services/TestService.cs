namespace Norm.Services
{
    public class TestService
    {
        private readonly bool botStarted;
        public TestService(BotService bot)
        {
            this.botStarted = bot.Started;
        }

        public void LogTestMessage(string testMessage)
        {
            if (this.botStarted)
                Serilog.Log.Debug(testMessage);
        }
    }
}
