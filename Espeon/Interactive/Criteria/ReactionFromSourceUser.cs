﻿using System.Threading.Tasks;
using Discord.WebSocket;
using Espeon.Commands;

namespace Espeon.Interactive.Criteria
{
    public class ReactionFromSourceUser : ICriterion<SocketReaction>
    {
        private readonly ulong _userId;

        public ReactionFromSourceUser(ulong userId)
        {
            _userId = userId;
        }

        public Task<bool> JudgeCriterionAsync(EspeonContext context, SocketReaction reaction)
            => Task.FromResult(reaction.UserId == _userId);
    }
}
