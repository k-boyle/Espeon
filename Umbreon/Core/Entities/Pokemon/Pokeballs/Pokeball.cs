namespace Umbreon.Core.Entities.Pokemon.Pokeballs
{
    public class Pokeball : BaseBall
    {
        public override int Cost { get; } = 1;
        public override int CatchRate { get; } = 255;
        public override string Name { get; } = "Pokeball";
    }
}
