namespace Umbreon.Core.Entities.Pokemon.Pokeballs
{
    public class MasterBall : BaseBall
    {
        public override int Cost => 100;
        public override int CatchRate => 0;
        public override string Name => "Master Ball";
    }
}