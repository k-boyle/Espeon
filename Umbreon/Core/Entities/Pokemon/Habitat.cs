using System.Collections.Generic;

namespace Umbreon.Core.Entities.Pokemon
{
    public class Habitat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IReadOnlyList<PokemonData> Pokemon { get; set; }
        public string ImageUrl { get; set; }
    }
}
