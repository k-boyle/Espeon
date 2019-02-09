﻿using Espeon.Database.Entities;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands.TypeParsers
{
    public class CustomCommandTypeParser : TypeParser<CustomCommand>
    {
        public override async Task<TypeParserResult<CustomCommand>> ParseAsync(string value, ICommandContext originalContext,
            IServiceProvider provider)
        {
            var context = originalContext as EspeonContext;

            var service = provider.GetService<CustomCommandsService>();
            var commands = await service.GetCommandsAsync(context);

            var found = commands.FirstOrDefault(x =>
                string.Equals(x.Name, value, StringComparison.InvariantCultureIgnoreCase));

            return found is null
                ? TypeParserResult<CustomCommand>.Unsuccessful("Failed to find command")
                : TypeParserResult<CustomCommand>.Successful(found);
        }
    }
}
