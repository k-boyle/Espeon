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
        public InteractiveService Interatice { get; }
        public IEmote Reaction { get; }
        public IUserMessage Message { get; }

        public DeleteCallback(ICommandContext context, InteractiveService interactive, IUserMessage message, IEmote reaction)
        {
            Context = context;
            Interatice = interactive;
            Message = message;
            Reaction = reaction;

            Interatice.AddReactionCallback(Message, this);
        }

        public void StartDelayAsync()
        {
            _ = Task.Delay(Timeout.GetValueOrDefault()).ContinueWith(_ =>
            {
                _ = Message.DeleteAsync();
                Interatice.RemoveReactionCallback(Message);
            });
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            if (!reaction.Emote.Equals(Reaction)) return false;
            await Message.DeleteAsync();
            Interatice.RemoveReactionCallback(Message);
            return true;
        }
    }
}
