using System;
using System.Collections.Generic;
using Serilog.Sinks.SystemConsole.Themes;

namespace Espeon {
    public static class EspeonLoggingConsoleTheme {
        public static SystemConsoleTheme Instance { get; } = new(
            new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle> {
                [ConsoleThemeStyle.Text] = new() {
                    Foreground = ConsoleColor.Gray
                },
                
                [ConsoleThemeStyle.SecondaryText] = new() {
                    Foreground = ConsoleColor.Magenta
                },
                
                [ConsoleThemeStyle.TertiaryText] = new() {
                    Foreground = ConsoleColor.DarkGray
                },
                
                [ConsoleThemeStyle.Invalid] = new() {
                    Foreground = ConsoleColor.Yellow
                },
                
                [ConsoleThemeStyle.Null] = new() {
                    Foreground = ConsoleColor.White
                },
                
                [ConsoleThemeStyle.Name] = new() {
                    Foreground = ConsoleColor.White
                },
                
                [ConsoleThemeStyle.String] = new() {
                    Foreground = ConsoleColor.White
                },
                
                [ConsoleThemeStyle.Number] = new() {
                    Foreground = ConsoleColor.White
                },
                
                [ConsoleThemeStyle.Boolean] = new() {
                    Foreground = ConsoleColor.White
                },
                
                [ConsoleThemeStyle.Scalar] = new() {
                    Foreground = ConsoleColor.White
                },
                
                [ConsoleThemeStyle.LevelVerbose] = new() {
                    Foreground = ConsoleColor.DarkGray
                },
                
                [ConsoleThemeStyle.LevelDebug] = new() {
                    Foreground = ConsoleColor.Green
                },
                
                [ConsoleThemeStyle.LevelInformation] = new() {
                    Foreground = ConsoleColor.Blue
                },
                
                [ConsoleThemeStyle.LevelWarning] = new() {
                    Foreground = ConsoleColor.Yellow
                },
                
                [ConsoleThemeStyle.LevelError] = new() {
                    Foreground = ConsoleColor.Red
                },
                
                [ConsoleThemeStyle.LevelFatal] = new() {
                    Foreground = ConsoleColor.Red
                }
            });
    }
}