using Discord;
using Espeon.Commands;
using Espeon.Databases.UserStore;
using System;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public interface ICandyService
    {
        Task UpdateCandiesAsync(EspeonContext context, IUser user, int amount)
            => UpdateCandiesAsync(context, context.UserStore, user, amount);

        Task UpdateCandiesAsync(EspeonContext context, UserStore store, IUser user, int amount);

        Task TransferCandiesAsync(EspeonContext context, IUser sender, IUser receiver, int amount);

        Task<int> GetCandiesAsync(EspeonContext context, IUser user);

        Task<(bool IsSuccess, int Amount, TimeSpan Cooldown)> TryClaimCandiesAsync(EspeonContext context, IUser toClaim);
    }
}
