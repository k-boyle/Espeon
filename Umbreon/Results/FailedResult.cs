using Discord.Commands;

namespace Umbreon.Results
{
    public class FailedResult : IResult
    {
        public CommandError? Error { get; }
        public string ErrorReason { get; }
        public bool IsSuccess { get; }

        public FailedResult(string errorReason, bool isSuccess, CommandError? error)
        {
            Error = error;
            ErrorReason = errorReason;
            IsSuccess = isSuccess;
        }
    }
}
