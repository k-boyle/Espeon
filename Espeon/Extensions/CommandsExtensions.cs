using Discord;
using Espeon.Commands.TypeParsers;
using Qmmands;
using System;
using System.Linq;
using System.Reflection;

namespace Espeon.Extensions
{
    public static partial class Extensions
    {
        public static CommandService AddTypeParsers(this CommandService commands, Assembly assembly)
        {
            var typeParserInterface = commands.GetType().Assembly.GetTypes()
                .FirstOrDefault(x => x.Name == "ITypeParser")?.GetTypeInfo();

            if (typeParserInterface is null)
                throw new QuahuRenamedException("ITypeParser");

            var parsers = assembly.GetTypes().Where(x => typeParserInterface.IsAssignableFrom(x));

            var internalAddParser = commands.GetType().GetMethod("AddParserInternal",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (internalAddParser is null)
                throw new QuahuRenamedException("AddParserInternal");

            foreach (var parser in parsers)
            {
                var @override = parser.GetCustomAttribute<DontOverrideAttribute>() is null;

                var targetType = parser.BaseType.GetGenericArguments().First();

                internalAddParser.Invoke(commands, new[] { targetType, Activator.CreateInstance(parser), !@override });
            }

            return commands;
        }

        //Based on https://github.com/discord-net/Discord.Net/blob/dev/src/Discord.Net.Commands/Extensions/MessageExtensions.cs#L45-L62
        public static bool HasMentionPrefix(this IMessage message, IUser user, out string parsed)
        {
            var content = message.Content;
            parsed = "";
            if (content.Length <= 3 || content[0] != '<' || content[1] != '@')
                return false;

            var endPos = content.IndexOf('>');
            if (endPos == -1) return false;

            if (content.Length < endPos + 2 || content[endPos + 1] != ' ')
                return false;

            if (!MentionUtils.TryParseUser(content.Substring(0, endPos + 1), out var userId))
                return false;

            if (userId != user.Id) return false;
            parsed = content.Substring(endPos + 2);
            return true;
        }

        public static string[] FindCommands(this string str)
        {
            var split = str.Split("::", StringSplitOptions.RemoveEmptyEntries);

            return split.Select(x => x.Trim()).ToArray();
        }
    }
}
