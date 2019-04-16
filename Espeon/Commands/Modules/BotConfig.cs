using System.Threading.Tasks;
using Qmmands;

namespace Espeon.Commands
{
    [Name("Bot Config")]
    [RequireOwner]
    public class BotConfig : EspeonBase
    {
        public Config Config { get; set; }

        [Command("SetRandomFrequency")]
        [Name("Set Random Frequency")]
        public Task SetRandomFrequencyAsync(float frequency)
        {
            Config.RandomCandyFrequency = frequency;
            return SendOkAsync(0);
        }

        [Command("SetRandomAmount")]
        [Name("Set Random Amount")]
        public Task SetRandomAmountAsync(int amount)
        {
            Config.RandomCandyAmount = amount;
            return SendOkAsync(0);
        }

        [Command("SetClaimMax")]
        [Name("Set Claim Max")]
        public Task SetClaimMaxAsync(int max)
        {
            Config.ClaimMax = max;
            return SendOkAsync(0);
        }

        [Command("SetClaimMin")]
        [Name("Set Claim Min")]
        public Task SetClaimMinAsync(int min)
        {
            Config.ClaimMin = min;
            return SendOkAsync(0);
        }

        [Command("SetClaimCooldown")]
        [Name("Set Claim Cooldown")]
        public Task SetClaimCooldownAsync(int cooldown)
        {
            Config.ClaimCooldown = cooldown;
            return SendOkAsync(0);
        }

        protected override Task AfterExecutedAsync()
        {
            Config.Serialize();
            return base.AfterExecutedAsync();
        }
    }
}
