namespace Espeon.Core.Entities
{
    public abstract class DatabaseEntity : IRemovable
    {
        public abstract ulong Id { get; }

        public abstract int TaskKey { get; set; }
        public abstract long WhenToRemove { get; set; }
    }
}
