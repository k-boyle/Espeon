using Discord.Commands;

namespace Umbreon.Interactive.Results
{
    public class OkResult : RuntimeResult
    {
        public OkResult(string reason = null) : base(null, reason) { }
    }
}
