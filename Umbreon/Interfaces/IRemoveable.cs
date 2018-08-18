using System;

namespace Umbreon.Interfaces
{
    public interface IRemoveable
    {
        int Identifier { get; }
        DateTime When { get; }
        IRemoveableService Service { get; }
    }
}
