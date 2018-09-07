using System.Collections.Generic;
using Umbreon.Core.Entities.Pokemon.Pokeballs;

namespace Umbreon.Core.Entities.User
{
    public class Bag
    {
        public List<BaseBall> PokeBalls { get; set; } = new List<BaseBall>
        {
            new Pokeball(),
            new Pokeball(),
            new Pokeball(),
            new Pokeball(),
            new Pokeball()
        };
    }
}
