using Espeon.Core.Commands.Bases;
using System.Threading.Tasks;

namespace Espeon.Core.Commands.Modules
{
    public abstract class Misc : EspeonBase
    {
        public abstract Task PingAsync();
    }
}
