namespace Espeon.Core.Entities
{
    public abstract class BaseReminder : IRemovable
    {
        public abstract string TheReminder { get; set; }
        public abstract string JumpUrl { get; set; }
        public abstract ulong GuildId { get; set; }
        public abstract ulong ChannelId { get; set; }
        public abstract ulong UserId { get; set; }
        public abstract int Id { get; set; }

        public long WhenToRemove { get; set; }
    }
}
