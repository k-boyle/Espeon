using System;

namespace Espeon
{
    public static class LogFactory
    {
        public static (Source Source, Severity Severity, string Message, Exception Exception) FromDiscord(
            Discord.LogMessage log)
        {
            return (Source.Discord, (Severity)(int)log.Severity, log.Message, log.Exception);
        }

        public static (Source Source, Severity Severity, string Message) FromPusharp(
            Pusharp.LogMessage log)
        {
            return (Source.Pusharp, 5 - (Severity)(int)log.Level, log.Message);
        }
    }
}
