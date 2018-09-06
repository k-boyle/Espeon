using System;
using System.Text;

namespace Umbreon.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string Humanize(this TimeSpan time)
            => $"{(time.Days > 0 ? $"{time.Days}days " : "")}{(time.Hours > 0 ? $"{time.Hours}hours " : "")}{(time.Minutes > 0 ? $"{time.Minutes}minutes " : "")}{(time.Seconds > 0 ? $"{time.Seconds}seconds " : "")}" +
               $"{(time < TimeSpan.FromSeconds(1) ? "1 second" : "")}";
    }
}
