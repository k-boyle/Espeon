using Discord;
using Espeon.Commands.TypeParsers;
using Espeon.Databases.CommandStore;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Espeon
{
    public static class Extensions
    {
        public static IServiceCollection AddServices(this IServiceCollection collection, IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                collection.AddSingleton(type);
            }

            return collection;
        }

        public static IServiceProvider Inject(this IServiceProvider services, IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                var service = services.GetService(type);

                Inject(services, service);
            }

            return services;
        }

        public static void Inject(this IServiceProvider services, object obj)
        {
            var members = obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0)
                .ToArray();

            foreach (var member in members)
            {
                Type type;
                object value;

                switch (member)
                {
                    case FieldInfo fieldInfo:
                        type = fieldInfo.FieldType;

                        value = services.GetService(type);

                        if (value is null)
                            continue;

                        fieldInfo.SetValue(obj, value);
                        break;

                    case PropertyInfo propertyInfo:
                        type = propertyInfo.PropertyType;

                        value = services.GetService(type);

                        if (value is null)
                            continue;

                        propertyInfo.SetValue(obj, value);
                        break;
                }
            }
        }

        public static async Task RunInitialisersAsync(this IServiceProvider services, UserStore userStore, GuildStore guildStore, CommandStore commandStore, IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                var service = services.GetService(type);

                if (!(service is BaseService validService))
                    throw new InvalidServiceException($"{type}");

                await validService.InitialiseAsync(userStore, guildStore, commandStore, services);
            }
        }

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

        public static string GetAvatarOrDefaultUrl(this IUser user)
        {
            return user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
        }

        public static string GetDisplayName(this IGuildUser user)
        {
            return user.Nickname ?? user.Username;
        }

        public static string[] FindCommands(this string str)
        {
            var split = str.Split("::", StringSplitOptions.RemoveEmptyEntries);

            return split.Select(x => x.Trim()).ToArray();
        }

        public static Task<T[]> AllAsync<T>(this IEnumerable<Task<T>> tasks)
            => Task.WhenAll(tasks);

        public static T Invoke<T>(this Action<T> action) where T : new()
        {
            var obj = new T();
            action.Invoke(obj);

            return obj;
        }
    }
}
