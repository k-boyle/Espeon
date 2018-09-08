using Umbreon.Attributes;

namespace Umbreon.Core.Entities.Pokemon.Pokeballs
{
    [ShopItem("Pokeball", "<:pokeball:487332258290860041>", 1)]
    public class NormalBall : BaseBall
    {
        public override int Cost { get; } = 1;
        public override int CatchRate { get; } = 255;
        public override string Name { get; } = "Pokeball";
    }
}
