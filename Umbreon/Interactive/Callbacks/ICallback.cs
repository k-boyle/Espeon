using Discord;
using System.Threading.Tasks;

namespace Umbreon.Interactive.Callbacks
{
    public interface ICallback
    {
        Task DisplayAsync();
        IUserMessage Message { get; }
    }
}
