using Umbreon.Attributes;

namespace Umbreon.Core.Entities.Pokemon.Pokeballs
{
    [ShopItem("Greatball", "<:greatball:487332755454164993>", 5)]
    public class GreatBall : BaseBall
    {
        public override int Cost => 5;
        public override int CatchRate => 200;
        public override string Name => "Great Ball";
    }
}
