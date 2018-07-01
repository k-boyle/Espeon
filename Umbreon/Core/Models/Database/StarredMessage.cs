namespace Umbreon.Core.Models.Database
{
    public class StarredMessage
    {
        public ulong MessageId { get; set; }
        public ulong StarMessageId { get; set; }
        public int StarCount { get; set; } = 0;
    }
}
