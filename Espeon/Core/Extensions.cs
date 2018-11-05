﻿using Espeon.Core.Attributes;
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
        public static IServiceCollection AddServices(this IServiceCollection collection, Assembly assembly, IEnumerable<Type> services = null)
        {
            services = services ?? assembly.FindTypesWithAttribute<ServiceAttribute>();

            foreach (var service in services)
            {
                var attribute = service.GetCustomAttribute<ServiceAttribute>();
                collection.AddSingleton(attribute.Target, service);
            }

            return collection;
        }

        public static IServiceProvider Inject(this IServiceProvider services, Assembly assembly, IEnumerable<Type> types = null)
        {
            types = types ?? assembly.FindTypesWithAttribute<ServiceAttribute>();

            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<ServiceAttribute>();
                var service = services.GetService(attribute.Target);

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

        public static IServiceProvider RunInitialisers(this IServiceProvider services, Assembly assembly, IEnumerable<Type> types = null)
        {
            types = types ?? FindTypesWithAttribute<ServiceAttribute>(assembly);

            foreach (var type in types)
            {
                var serviceAtt = type.GetCustomAttribute<ServiceAttribute>();

                var service = services.GetService(serviceAtt.Target);

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
                .FirstOrDefault(x => x.Name == "ITypeParser").GetTypeInfo();

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
    }
}
