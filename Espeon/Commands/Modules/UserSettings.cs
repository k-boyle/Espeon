using Espeon.Databases.Entities;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Commands.Modules
{
    [Name("User Settings")]
    public class UserSettings : EspeonBase
    {
        [Command("setresponses")]
        [Name("Set Responses")]
        public async Task SetResponsesAsync(ResponsePack pack = ResponsePack.Default)
        {
            var foundUser = await Context.UserStore.GetOrCreateUserAsync(Context.User);
            foundUser.ResponsePack = pack;

            await Context.UserStore.SaveChangesAsync();

            await SendOkAsync(0, pack);
        }
    }
}
