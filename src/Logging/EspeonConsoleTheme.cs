using System;
using System.Collections.Generic;
using System.ComponentModel;
using Serilog.Sinks.SystemConsole.Themes;

namespace Espeon.Logging
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class EspeonLoggingConsoleTheme
    {
        public static SystemConsoleTheme Instance { get; } = new(
            new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
            {
                [ConsoleThemeStyle.Text] = new()
                {
                    Foreground = ConsoleColor.Gray
                },

                [ConsoleThemeStyle.SecondaryText] = new()
                {
                    Foreground = ConsoleColor.DarkMagenta
                },

                [ConsoleThemeStyle.TertiaryText] = new()
                {
                    Foreground = ConsoleColor.DarkGray
                },

                [ConsoleThemeStyle.Invalid] = new()
                {
                    Foreground = ConsoleColor.Yellow
                },

                [ConsoleThemeStyle.Null] = new()
                {
                    Foreground = ConsoleColor.Cyan
                },

                [ConsoleThemeStyle.Name] = new()
                {
                    Foreground = ConsoleColor.Cyan
                },

                [ConsoleThemeStyle.String] = new()
                {
                    Foreground = ConsoleColor.Cyan
                },

                [ConsoleThemeStyle.Number] = new()
                {
                    Foreground = ConsoleColor.Yellow
                },

                [ConsoleThemeStyle.Boolean] = new()
                {
                    Foreground = ConsoleColor.Yellow
                },

                [ConsoleThemeStyle.Scalar] = new()
                {
                    Foreground = ConsoleColor.Yellow
                },

                [ConsoleThemeStyle.LevelVerbose] = new()
                {
                    Foreground = ConsoleColor.DarkGray
                },

                [ConsoleThemeStyle.LevelDebug] = new()
                {
                    Foreground = ConsoleColor.Green
                },

                [ConsoleThemeStyle.LevelInformation] = new()
                {
                    Foreground = ConsoleColor.Blue
                },

                [ConsoleThemeStyle.LevelWarning] = new()
                {
                    Foreground = ConsoleColor.Yellow
                },

                [ConsoleThemeStyle.LevelError] = new()
                {
                    Foreground = ConsoleColor.Red
                },

                [ConsoleThemeStyle.LevelFatal] = new()
                {
                    Foreground = ConsoleColor.Red
                }
            });
    }
}