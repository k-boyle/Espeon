namespace Espeon.Databases
{
    public class MockWebhook
    {
        public Guild Guild { get; set; }
        public ulong GuildId { get; set; }

        public ulong Id { get; set; }
        public string Token { get; set; }

        public ulong ChannelId { get; set; }
    }
}
