using System;

namespace Umbreon.Interfaces
{
    public interface IRemoveable
    {
        TimeSpan When { get; }
        IRemoveableService Service { get; }
    }
}
