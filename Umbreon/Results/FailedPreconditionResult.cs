using Discord.Commands;

namespace Umbreon.Results
{
    public class FailedPreconditionResult : IResult
    {
        public CommandError? Error { get; }
        public string ErrorReason { get; }
        public bool IsSuccess { get; }

        public FailedPreconditionResult(string errorReason, bool isSuccess, CommandError? error)
        {
            Error = error;
            ErrorReason = errorReason;
            IsSuccess = isSuccess;
        }
    }
}
