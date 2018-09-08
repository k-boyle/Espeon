using Umbreon.Attributes;

namespace Umbreon.Core.Entities.Pokemon.Pokeballs
{
    [ShopItem("Masterball", "<:masterball:488077901632372736>", 100)]
    public class MasterBall : BaseBall
    {
        public override int Cost => 100;
        public override int CatchRate => 0;
        public override string Name => "Master Ball";
    }
}