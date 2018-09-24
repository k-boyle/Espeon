using Discord;
using System.Threading.Tasks;

namespace Espeon.Interactive.Callbacks
{
    public interface ICallback
    {
        Task DisplayAsync();
        IUserMessage Message { get; }
    }
}
