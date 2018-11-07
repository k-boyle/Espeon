namespace Espeon.Core.Entities
{
    public interface IRemovable
    {
        int TaskKey { get; }
        long WhenToRemove { get; }
    }
}
