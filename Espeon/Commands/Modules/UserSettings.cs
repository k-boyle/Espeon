using Espeon.Services;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    [Name("User Settings")]
    public class UserSettings : EspeonBase
    {
        public CandyService Candy { get; set; }

        [Command("setresponses")]
        [Name("Set Responses")]
        public async Task SetResponsesAsync([RequireUnlocked] ResponsePack pack = ResponsePack.Default)
        {
            var foundUser = await Context.GetInvokerAsync();
            foundUser.ResponsePack = pack;

            await Task.WhenAll(Context.UserStore.SaveChangesAsync(), SendOkAsync(0, pack));
        }

        [Command("buyowo")]
        [Name("Buy owo")]
        public async Task BuyOwoAsync()
        {
            var user = await Context.GetInvokerAsync();

            if(user.ResponsePacks.Contains(ResponsePack.owo))
            {
                await SendNotOkAsync(0);
                return;
            }

            if(user.CandyAmount < 5000)
            {
                await SendNotOkAsync(1);
                return;
            }

            await Candy.UpdateCandiesAsync(Context, user.Id, -5000);

            user.ResponsePacks.Add(ResponsePack.owo);

            await Task.WhenAll(Context.UserStore.SaveChangesAsync(), SendOkAsync(2));
        }
    }
}
