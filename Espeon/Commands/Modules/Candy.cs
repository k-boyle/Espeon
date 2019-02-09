using Discord;
using Discord.WebSocket;
using Espeon.Services;
using Humanizer;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Commands.Modules
{
    /*
     * Candies
     * Claim
     * House
     * Treat
     * Leaderboard
     * Gift
     * Steal
     */

    [Name("Candy Commands")]
    public class Candy : EspeonBase
    {
        public CandyService CandyService { get; set; }
        public EmotesService Emotes { get; set; }

        public Emote RareCandy => Emotes.Collection["RareCandy"];

        [Command("Candies")]
        [Name("View Candies")]
        public async Task ViewCandiesAsync([Remainder] SocketGuildUser user = null)
        {
            user ??= Context.User;

            var amount = await CandyService.GetCandiesAsync(Context, user.Id);

            await SendOkAsync($"{user.GetDisplayName()} has {amount} rare cand{(amount == 1 ? "y" : "ies")}{RareCandy}!");
        }

        [Command("Claim")]
        [Name("Claim Candies")]
        public async Task ClaimCandiesAsync()
        {
            var (IsSuccess, Amount, Cooldown) = await CandyService.TryClaimCandiesAsync(Context, Context.User.Id);

            if(IsSuccess)
            {
                await SendOkAsync($"You have received {Amount} rare cand{(Amount == 1 ? "y" : "ies")}{RareCandy}!");
                return;
            }

            await SendNotOkAsync($"You recently claimed your candies, please wait {Cooldown.Humanize(2)}");
        }
    }
}
