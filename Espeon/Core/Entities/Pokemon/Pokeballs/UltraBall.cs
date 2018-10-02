using Espeon.Attributes;

namespace Espeon.Core.Entities.Pokemon.Pokeballs
{
    [ShopItem("Ultraball", "<:ultraball:487333008601251850>", 10)]
    public class UltraBall : BaseBall
    {
        public override int Cost => 10;
        public override int CatchRate => 150;
        public override string Name => "Ultra Ball";
    }
}
