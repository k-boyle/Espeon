using Discord;
using System;

namespace Espeon.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ShopItemAttribute : Attribute
    {
        public string ItemName { get; }
        public Emote Emote { get; }
        public int Price { get; }

        public ShopItemAttribute(string itemName, string emote, int price)
        {
            ItemName = itemName;
            Emote = Emote.Parse(emote);
            Price = price;
        }
    }
}
