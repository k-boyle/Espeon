using System.Threading.Tasks;
using Discord;

namespace Umbreon.Interactive.Callbacks
{
    public interface ICallback
    {
        Task DisplayAsync();
        IUserMessage Message { get; }
    }
}
