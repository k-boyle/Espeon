using Espeon.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Espeon.Core
{
    public static class Extensions
    {
        public static IServiceCollection AddServices(this IServiceCollection collection, Assembly assembly)
        {
            var services = assembly.FindTypesWithAttribute<ServiceAttribute>();

            foreach (var service in services)
            {
                var attribute = service.GetCustomAttribute<ServiceAttribute>();
                collection.AddSingleton(service, attribute.Target);
            }

            return collection;
        }

        public static IServiceProvider Inject(this IServiceProvider services, Assembly assembly)
        {
            var types = assembly.FindTypesWithAttribute<ServiceAttribute>();

            foreach (var type in types)
            {
                var service = services.GetService(type);

                Inject(services, service);
            }

            return services;
        }

        public static void Inject(this IServiceProvider services, object obj)
        {
            var fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public)
                .Where(x => x.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0)
                .ToArray();

            foreach (var field in fields)
            {
                var type = field.FieldType;

                var value = services.GetService(type);

                if (value is null)
                    continue;

                field.SetValue(obj, value);
            }

            var properties = obj.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public)
                .Where(x => x.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0)
                .ToArray();

            foreach (var property in properties)
            {
                var type = property.PropertyType;

                var value = services.GetService(type);

                if (value is null)
                    continue;

                property.SetValue(obj, value);
            }
        }

        public static IServiceProvider RunInitialisers(this IServiceProvider services, Assembly assembly)
        {
            var types = FindTypesWithAttribute<ServiceAttribute>(assembly);

            foreach (var type in types)
            {
                var service = services.GetService(type);

                foreach (var method in service.GetType().GetMethods())
                {
                    if (!(method.GetCustomAttribute<InitialiserAttribute>() is InitialiserAttribute attribute))
                        continue;

                    var argTypes = attribute.Arguments;
                    var args = argTypes.Select(services.GetService).ToArray();
                    method.Invoke(service, args);
                }
            }

            return services;
        }

        private static IEnumerable<Type> FindTypesWithAttribute<T>(this Assembly assembly)
        {
            return assembly.GetTypes().Where(x => x.GetCustomAttributes(typeof(T), true).Length > 0);
        }

        public static CommandService AddTypeParsers(this CommandService commands)
        {
            var typeParserInterface = commands.GetType().Assembly.GetTypes()
                .FirstOrDefault(x => x.Name == "ITypeParser").GetTypeInfo();

            if (typeParserInterface is null)
                throw new QuahuRenamedException("ITypeParser");

            var parsers = Assembly.GetEntryAssembly().GetTypes()
                .Where(x => typeParserInterface.IsAssignableFrom(x));

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
    }
}
