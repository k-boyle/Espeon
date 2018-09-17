using System;
using System.Collections.Generic;
using Espeon.Core.Entities.Pokemon;
using Espeon.Interfaces;

namespace Espeon.Core.Entities.User
{
    public class UserObject : BaseObject
    {
        public UserObject(BaseObject baseObj, IRemoveableService service) : base(baseObj, service)
        {
        }

        public UserObject() { }

        public int RareCandies { get; set; } = 10;
        public DateTime LastClaimed { get; set; } = DateTime.UtcNow.AddDays(-1);

        public PlayingData Data { get; set; } = new PlayingData();

        public Bag Bag { get; set; } = new Bag();

        public List<PokedexEntry> Pokedex { get; set; } = new List<PokedexEntry>();
    }
}
