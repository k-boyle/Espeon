using Discord.Commands;

namespace Umbreon.Results
{
    public class WrongGuildResult : IResult
    {
        public CommandError? Error { get; }
        public string ErrorReason { get; }
        public bool IsSuccess { get; }

        public WrongGuildResult(string errorReason, bool isSuccess, CommandError? error)
        {
            Error = error;
            ErrorReason = errorReason;
            IsSuccess = isSuccess;
        }
    }
}
