using System.Threading.Tasks;
using Qmmands;

namespace Espeon.Commands
{
    [Name("Bot Config")]
    [RequireOwner]
    [Description("Change how the bot behaves")]
    public class BotConfig : EspeonBase
    {
        public Config Config { get; set; }

        [Command("SetRandomFrequency")]
        [Name("Set Random Frequency")]
        [Description("Set how frequently message based candies are added")]
        public Task SetRandomFrequencyAsync(float frequency)
        {
            Config.RandomCandyFrequency = frequency;
            return SendOkAsync(0);
        }

        [Command("SetRandomAmount")]
        [Name("Set Random Amount")]
        [Description("Set the upper bound of message based candies")]
        public Task SetRandomAmountAsync(int amount)
        {
            Config.RandomCandyAmount = amount;
            return SendOkAsync(0);
        }

        [Command("SetClaimMax")]
        [Name("Set Claim Max")]
        [Description("Set the upper bound of claim based candies")]
        public Task SetClaimMaxAsync(int max)
        {
            Config.ClaimMax = max;
            return SendOkAsync(0);
        }

        [Command("SetClaimMin")]
        [Name("Set Claim Min")]
        [Description("Set the lower bound of claim based candies")]
        public Task SetClaimMinAsync(int min)
        {
            Config.ClaimMin = min;
            return SendOkAsync(0);
        }

        [Command("SetClaimCooldown")]
        [Name("Set Claim Cooldown")]
        [Description("Set how frequently candies can be claimed")]
        public Task SetClaimCooldownAsync(int cooldown)
        {
            Config.ClaimCooldown = cooldown;
            return SendOkAsync(0);
        }

        [Command("SetPackPrice")]
        [Name("Set Pack Price")]
        [Description("Set the price of response packs")]
        public Task SetPackPriceAsync(int price)
        {
            Config.PackPrice = price;
            return SendOkAsync(0);
        }

        protected override Task AfterExecutedAsync()
        {
            Config.Serialize();
            return base.AfterExecutedAsync();
        }
    }
}
