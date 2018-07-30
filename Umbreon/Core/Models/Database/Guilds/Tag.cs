using System;

namespace Umbreon.Core.Models.Database
{
    public class Tag
    {
        public string TagName { get; set; }
        public string TagValue { get; set; }
        public ulong TagOwner { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Uses { get; set; }
    }
}
