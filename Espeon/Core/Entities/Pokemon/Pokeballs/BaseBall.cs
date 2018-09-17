namespace Espeon.Core.Entities.Pokemon.Pokeballs
{
    public abstract class BaseBall
    {
        public abstract int Cost { get; }
        public abstract int CatchRate { get; }
        public abstract string Name { get; }
    }
}
