using System;
using System.Threading.Tasks;

namespace Espeon.Interfaces
{
    public interface IRemoveable
    {
        int Identifier { get; }
        DateTime When { get; }
        Task RemoveAsync();
    }
}
