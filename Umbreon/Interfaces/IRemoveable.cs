using System;
using System.Threading.Tasks;

namespace Umbreon.Interfaces
{
    public interface IRemoveable
    {
        int Identifier { get; }
        DateTime When { get; }
        Task RemoveAsync();
    }
}
