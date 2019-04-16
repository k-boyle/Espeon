using Espeon.Services;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    [Name("User Settings")]
    public class UserSettings : EspeonBase
    {
        public CandyService Candy { get; set; }
        public Config Config { get; set; }

        [Command("setresponses")]
        [Name("Set Responses")]
        public async Task SetResponsesAsync([RequireUnlocked] ResponsePack pack = ResponsePack.Default)
        {
            var foundUser = await Context.GetInvokerAsync();
            foundUser.ResponsePack = pack;
            Context.UserStore.Update(foundUser);

            await Task.WhenAll(Context.UserStore.SaveChangesAsync(), SendOkAsync(0, pack));
        }

        [Command("buy")]
        [Name("Buy")]
        public async Task BuyOwo(ResponsePack pack)
        {
            var user = await Context.GetInvokerAsync();

            if(user.ResponsePacks.Contains(pack))
            {
                await SendNotOkAsync(0);
                return;
            }

            if(user.CandyAmount < Config.PackPrice)
            {
                await SendNotOkAsync(1, Config.PackPrice);
                return;
            }

            await Candy.UpdateCandiesAsync(Context, user.Id, -Config.PackPrice);

            user.ResponsePacks.Add(pack);
            Context.UserStore.Update(user);

            await Task.WhenAll(Context.UserStore.SaveChangesAsync(), SendOkAsync(2, pack));
        }
    }
}
