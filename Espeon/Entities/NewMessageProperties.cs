using Discord;
using System.IO;

namespace Espeon.Entities
{
    public class NewMessageProperties
    {
        public string Content { get; set; }
        public Embed Embed { get; set; }
        public bool IsTTS { get; set; }
        public Stream Stream { get; set; }
        public string FileName { get; set; }
        public bool IsSpoiler { get; set; }
    }
}
