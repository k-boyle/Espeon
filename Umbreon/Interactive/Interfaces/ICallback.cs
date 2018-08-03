using System.Threading.Tasks;
using Discord;

namespace Umbreon.Interactive.Interfaces
{
    public interface ICallback
    {
        Task DisplayAsync();
        IUserMessage Message { get; }
    }
}
