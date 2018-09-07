namespace Umbreon.Core.Entities.Pokemon.Pokeballs
{
    public class GreatBall : BaseBall
    {
        public override int Cost => 5;
        public override int CatchRate => 200;
        public override string Name => "Great Ball";
    }
}
