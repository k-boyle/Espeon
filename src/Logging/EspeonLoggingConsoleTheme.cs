using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;

namespace Espeon {
    public static class EspeonLoggingConsoleTheme {
        public static SystemConsoleTheme Instance { get; } = new SystemConsoleTheme(
            new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle> {
                [ConsoleThemeStyle.Text] = new SystemConsoleThemeStyle {
                    Foreground = ConsoleColor.Gray
                },
                
                [ConsoleThemeStyle.SecondaryText] = new SystemConsoleThemeStyle {
                    Foreground = ConsoleColor.Magenta
                },
                
                [ConsoleThemeStyle.TertiaryText] = new SystemConsoleThemeStyle {
                    Foreground = ConsoleColor.DarkGray
                },
                
                [ConsoleThemeStyle.Invalid] = new SystemConsoleThemeStyle {
                    Foreground = ConsoleColor.Yellow
                },
                
                [ConsoleThemeStyle.Null] = new SystemConsoleThemeStyle {
                    Foreground = ConsoleColor.White
                },
                
                [ConsoleThemeStyle.Name] = new SystemConsoleThemeStyle {
                    Foreground = ConsoleColor.White
                },
                
                [ConsoleThemeStyle.String] = new SystemConsoleThemeStyle {
                    Foreground = ConsoleColor.White
                },
                
                [ConsoleThemeStyle.Number] = new SystemConsoleThemeStyle {
                    Foreground = ConsoleColor.White
                },
                
                [ConsoleThemeStyle.Boolean] = new SystemConsoleThemeStyle {
                    Foreground = ConsoleColor.White
                },
                
                [ConsoleThemeStyle.Scalar] = new SystemConsoleThemeStyle {
                    Foreground = ConsoleColor.White
                },
                
                [ConsoleThemeStyle.LevelVerbose] = new SystemConsoleThemeStyle {
                    Foreground = ConsoleColor.DarkGray
                },
                
                [ConsoleThemeStyle.LevelDebug] = new SystemConsoleThemeStyle {
                    Foreground = ConsoleColor.Green
                },
                
                [ConsoleThemeStyle.LevelInformation] = new SystemConsoleThemeStyle {
                    Foreground = ConsoleColor.Blue
                },
                
                [ConsoleThemeStyle.LevelWarning] = new SystemConsoleThemeStyle {
                    Foreground = ConsoleColor.Yellow
                },
                
                [ConsoleThemeStyle.LevelError] = new SystemConsoleThemeStyle {
                    Foreground = ConsoleColor.Red
                },
                
                [ConsoleThemeStyle.LevelFatal] = new SystemConsoleThemeStyle {
                    Foreground = ConsoleColor.Red
                },
            });
    }
}