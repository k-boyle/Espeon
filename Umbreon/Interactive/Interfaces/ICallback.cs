using Discord;
using System.Threading.Tasks;

namespace Umbreon.Interactive.Interfaces
{
    public interface ICallback
    {
        Task DisplayAsync();
        IUserMessage Message { get; }
    }
}
