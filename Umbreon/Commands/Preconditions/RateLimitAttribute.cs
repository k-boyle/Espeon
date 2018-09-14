using Discord;
using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Umbreon.Commands.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public sealed class RatelimitAttribute : PreconditionAttribute
    {
        private readonly bool _applyPerGuild;
        private readonly uint _invokeLimit;
        private readonly TimeSpan _invokeLimitPeriod;

        private readonly ConcurrentDictionary<(ulong, ulong?), CommandTimeout> _invokeTracker =
            new ConcurrentDictionary<(ulong, ulong?), CommandTimeout>();

        private readonly bool _noLimitForAdmins;
        private readonly bool _noLimitInDMs;

        public RatelimitAttribute(
            uint times,
            double period,
            Measure measure,
            RatelimitFlags flags = RatelimitFlags.None)
        {
            _invokeLimit = times;
            _noLimitInDMs = (flags & RatelimitFlags.NoLimitInDMs) == RatelimitFlags.NoLimitInDMs;
            _noLimitForAdmins = (flags & RatelimitFlags.NoLimitForAdmins) == RatelimitFlags.NoLimitForAdmins;
            _applyPerGuild = (flags & RatelimitFlags.ApplyPerGuild) == RatelimitFlags.ApplyPerGuild;

            switch (measure)
            {
                case Measure.Days:
                    _invokeLimitPeriod = TimeSpan.FromDays(period);
                    break;
                case Measure.Hours:
                    _invokeLimitPeriod = TimeSpan.FromHours(period);
                    break;
                case Measure.Minutes:
                    _invokeLimitPeriod = TimeSpan.FromMinutes(period);
                    break;
                case Measure.Seconds:
                    _invokeLimitPeriod = TimeSpan.FromSeconds(period);
                    break;
            }
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(
            ICommandContext context,
            CommandInfo command,
            IServiceProvider services)
        {
            if (_noLimitInDMs && context.Channel is IPrivateChannel)
                return Task.FromResult(PreconditionResult.FromSuccess(command));

            if (_noLimitForAdmins && context.User is IGuildUser gu && gu.GuildPermissions.Administrator)
                return Task.FromResult(PreconditionResult.FromSuccess(command));

            var now = DateTime.UtcNow;
            var key = _applyPerGuild ? (context.User.Id, context.Guild?.Id) : (context.User.Id, null);

            var timeout = _invokeTracker.TryGetValue(key, out var t)
                          && now - t.FirstInvoke < _invokeLimitPeriod
                ? t
                : new CommandTimeout(now);

            timeout.TimesInvoked++;

            if (timeout.TimesInvoked > _invokeLimit)
                return Task.FromResult(PreconditionResult.FromError(command, 
                    $"This command is on cooldown please wait {(timeout.FirstInvoke.Add(_invokeLimitPeriod) - DateTime.UtcNow).Seconds}s"));
            _invokeTracker[key] = timeout;
            return Task.FromResult(PreconditionResult.FromSuccess(command));
        }

        private sealed class CommandTimeout
        {
            public CommandTimeout(DateTime timeStarted)
            {
                FirstInvoke = timeStarted;
            }

            public uint TimesInvoked { get; set; }
            public DateTime FirstInvoke { get; }
        }
    }

    public enum Measure
    {
        Days,
        Hours,
        Minutes,
        Seconds
    }

    [Flags]
    public enum RatelimitFlags
    {
        None = 0,
        NoLimitInDMs = 1 << 0,
        NoLimitForAdmins = 1 << 1,
        ApplyPerGuild = 1 << 2
    }
}