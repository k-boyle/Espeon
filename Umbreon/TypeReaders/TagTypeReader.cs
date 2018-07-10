using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Umbreon.Helpers;
using Umbreon.Services;

namespace Umbreon.TypeReaders
{
    public class TagTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var tagService = services.GetService<TagService>();
            var currentTags = tagService.GetTags(context);
            var levenTags = currentTags.Where(x => StringHelper.CalcLevenshteinDistance(x.TagName, input) < 5);
            var containsTags = currentTags.Where(x => x.TagName.Contains(input));
            var totalTags = levenTags.Concat(containsTags);
            return Task.FromResult(tagService.TryParse(currentTags, input, out var foundTag) ? TypeReaderResult.FromSuccess(foundTag) : TypeReaderResult.FromError(CommandError.ParseFailed, 
                ("Tag not found did you mean?\n" + $"{string.Join("\n", totalTags.Select(x => x.TagName))}")));
        }
    }
}
