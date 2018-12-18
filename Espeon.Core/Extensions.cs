using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Discord;
using Espeon.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Espeon.Core
{
    public static class Extensions
    {
        public static IServiceCollection AddServices(this IServiceCollection collection, Assembly assembly)
            => AddServices(collection, assembly.FindTypesWithAttribute<ServiceAttribute>());

        public static IServiceCollection AddServices(this IServiceCollection collection, IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<ServiceAttribute>();
                var target = attribute.Generic is null ? attribute.Target : attribute.Target.MakeGenericType(attribute.Generic);
                collection.AddSingleton(target, type);
            }

            return collection;
        }

        public static IServiceProvider Inject(this IServiceProvider services, Assembly assembly)
            => Inject(services, assembly.FindTypesWithAttribute<ServiceAttribute>());

        public static IServiceProvider Inject(this IServiceProvider services, IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<ServiceAttribute>();
                var target = attribute.Generic is null ? attribute.Target : attribute.Target.MakeGenericType(attribute.Generic);
                var service = services.GetService(target);

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

        public static IServiceProvider RunInitialisers(this IServiceProvider services, Assembly assembly)
            => RunInitialisers(services, FindTypesWithAttribute<ServiceAttribute>(assembly));

        public static IServiceProvider RunInitialisers(this IServiceProvider services, IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                var serviceAtt = type.GetCustomAttribute<ServiceAttribute>();
                var target = serviceAtt.Generic is null ? serviceAtt.Target : serviceAtt.Target.MakeGenericType(serviceAtt.Generic);
                var service = services.GetService(target);

                foreach (var method in service.GetType().GetMethods())
                {
                    if (!(method.GetCustomAttribute<InitialiserAttribute>() is InitialiserAttribute initAtt))
                        continue;

                    var argTypes = initAtt.Arguments;
                    var args = argTypes.Select(services.GetService).ToArray();
                    method.Invoke(service, args);
                }
            }

            return services;
        }

        public static IEnumerable<Type> FindTypesWithAttribute<T>(this Assembly assembly)
        {
            return assembly.GetTypes().Where(x => x.GetCustomAttributes(typeof(T), true).Length > 0);
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
                var targetType = parser.BaseType.GetGenericArguments().First();

                internalAddParser.Invoke(commands, new[] {targetType, Activator.CreateInstance(parser), true});
            }

            return commands;
        }

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

        public static string GetAvatarOrDefaultUrl(this IGuildUser user)
        {
            return user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
        }

        public static string GetDisplayName(this IGuildUser user)
        {
            return user.Nickname ?? user.Username;
        }
    }
}
