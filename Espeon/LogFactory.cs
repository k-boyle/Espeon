using Espeon.Core;
using System;
using System.Collections.Generic;

namespace Espeon
{
    public static class LogFactory
    {
        private static readonly IReadOnlyDictionary<Discord.LogSeverity, Severity> DiscordSeverity =
            new Dictionary<Discord.LogSeverity, Severity>
        {
            { Discord.LogSeverity.Verbose, Severity.Verbose },
            { Discord.LogSeverity.Critical, Severity.Critical },
            { Discord.LogSeverity.Debug, Severity.Debug },
            { Discord.LogSeverity.Error, Severity.Error },
            { Discord.LogSeverity.Info, Severity.Info },
            { Discord.LogSeverity.Warning, Severity.Verbose },
        };

        private static readonly IReadOnlyDictionary<Pusharp.LogLevel, Severity> PusharpSeverity =
            new Dictionary<Pusharp.LogLevel, Severity>
        {
            { Pusharp.LogLevel.Verbose, Severity.Verbose },
            { Pusharp.LogLevel.Critical, Severity.Critical },
            { Pusharp.LogLevel.Debug, Severity.Debug },
            { Pusharp.LogLevel.Error, Severity.Error },
            { Pusharp.LogLevel.Info, Severity.Info },
            { Pusharp.LogLevel.Warning, Severity.Warning }
        };

        public static (Source Source, Severity Severity, string Message, Exception Exception) FromDiscord(
            Discord.LogMessage log)
        {
            return (Source.Discord, DiscordSeverity[log.Severity], log.Message, log.Exception);
        }

        public static (Source Source, Severity Severity, string Message) FromPusharp(
            Pusharp.LogMessage log)
        {
            return (Source.Pusharp, PusharpSeverity[log.Level], log.Message);
        }
    }
}
