using System;

namespace Umbreon.Core.Entities.User
{
    public class UserObject : BaseObject
    {
        public int RareCandies { get; set; } = 10;
        public DateTime LastClaimed { get; set; } = DateTime.UtcNow.AddDays(-1);

        public PlayingData Data { get; set; } = new PlayingData();
    }
}
