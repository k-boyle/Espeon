namespace Espeon.Core.Entities
{
    public abstract class DatabaseEntity : IRemovable
    {
        public abstract ulong Id { get; set; }
        
        //unix milliseconds to account for LiteDb being stupid with handling datetimes
        public abstract long WhenToRemove { get; set; }
    }
}
