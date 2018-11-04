using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Discord;
using Espeon.Interactive;
using Espeon.Interactive.Callbacks;
using Espeon.Interactive.Criteria;
using Espeon.Interactive.Paginator;

namespace Espeon.Callbacks
{
    public class DeleteCallback : IReactionCallback
    {
        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion => new EnsureReactionFromSourceUserCriterion();
        public TimeSpan? Timeout => TimeSpan.FromMinutes(2);
        public ICommandContext Context { get; }
        public InteractiveService Interative { get; }
        public IEmote Reaction { get; }
        public IUserMessage Message { get; }

        public DeleteCallback(ICommandContext context, InteractiveService interactive, IUserMessage message, IEmote reaction)
        {
            Context = context;
            Interative = interactive;
            Message = message;
            Reaction = reaction;

            Interative.AddReactionCallback(Message, this);
        }

        public void StartDelayAsync()
        {
            _ = Task.Delay(Timeout.GetValueOrDefault()).ContinueWith(_ =>
            {
                _ = Message.DeleteAsync();
                Interative.RemoveReactionCallback(Message);
            });
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            if (!reaction.Emote.Equals(Reaction)) return false;
            await Message.DeleteAsync();
            Interative.RemoveReactionCallback(Message);
            return true;
        }
    }
}
