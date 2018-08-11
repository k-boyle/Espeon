using System;

namespace Umbreon.Interfaces
{
    public interface IRemoveable
    {
        int Identifier { get; }
        TimeSpan When { get; }
        IRemoveableService Service { get; }
    }
}
