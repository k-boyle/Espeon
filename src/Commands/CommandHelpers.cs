using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Espeon {
    public static class CommandHelpers {
        public static async Task GlobalTagCallback(EspeonCommandContext context) {
            await using var dbContext = context.ServiceProvider.GetService<EspeonDbContext>();
            var tag = await dbContext.GetTagAsync<GlobalTag>(context.Command.Name);
            await context.Channel.SendMessageAsync(tag.Value);
            tag.Uses++;
            await dbContext.UpdateAsync(tag);
        }
    }
}