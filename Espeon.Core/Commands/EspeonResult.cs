using Qmmands;

namespace Espeon.Core.Commands
{
    public class EspeonResult : CommandResult
    {
        public override bool IsSuccessful { get; }
        public string Message { get; }

        public EspeonResult(bool isSuccessful, string message)
        {
            IsSuccessful = isSuccessful;
            Message = message;
        }
    }
}
