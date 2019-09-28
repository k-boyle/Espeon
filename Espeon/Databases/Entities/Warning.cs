namespace Espeon.Databases
{
    public class Warning
    {
        public Guild Guild { get; set; }
        public ulong GuildId { get; set; }

        public string Id { get; set; }

        public ulong TargetUser { get; set; }
        public ulong Issuer { get; set; }
        public string Reason { get; set; }
        public long IssuedOn { get; set; }
    }
}
