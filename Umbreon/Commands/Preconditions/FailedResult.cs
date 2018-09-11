using Discord.Commands;

namespace Umbreon.Commands.Preconditions
{
    public class FailedResult : IResult
    {
        public CommandError? Error { get; }
        public string ErrorReason { get; }
        public bool IsSuccess { get; } = false;

        public FailedResult(string errorReason, CommandError? error)
        {
            Error = error;
            ErrorReason = errorReason;
        }
    }
}
