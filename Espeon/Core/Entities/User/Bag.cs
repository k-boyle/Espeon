using System.Collections.Generic;
using Espeon.Core.Entities.Pokemon.Pokeballs;

namespace Espeon.Core.Entities.User
{
    public class Bag
    {
        public List<BaseBall> PokeBalls { get; set; } = new List<BaseBall>
        {
            new NormalBall(),
            new NormalBall(),
            new NormalBall(),
            new NormalBall(),
            new NormalBall()
        };
    }
}
