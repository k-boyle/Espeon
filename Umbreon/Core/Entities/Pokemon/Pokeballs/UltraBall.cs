namespace Umbreon.Core.Entities.Pokemon.Pokeballs
{
    public class UltraBall : BaseBall
    {
        public override int Cost => 10;
        public override int CatchRate => 3;
        public override string Name => "Ultra Ball";
    }
}
