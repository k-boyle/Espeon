namespace Umbreon.Core.Entities.Pokemon
{
    public class PokedexEntry
    {
        public int Id { get; set;}
        public int Count { get; set; }
        public bool Caught { get; set; } = false;
    }
}