using LiteDB;

namespace Umbreon.Core.Models.Database
{
    public class BotConfig
    {
        [BsonId(false)]
        public int Index { get; set; }
        public string BotToken { get; set; }
        public string Giphy { get; set; }
    }
}
