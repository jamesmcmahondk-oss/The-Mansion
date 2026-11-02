using System;
using System.Collections.Generic;
using The_Mansion.Models;

namespace The_Mansion.Data
{
    /*
     * Every item in the game. Add new items here, then give them to the
     * player with  p.Inventory.Add("Item Name")  (e.g. in a dialogue Effect).
     * Icons are built by Tools/build_item_icons.py.
     */
    public static class Items
    {
        public static Item GoldCoins { get; } = new Item
        {
            Name = "Gold Coins",
            Description = "A dripping handful of gold coins from the fountain. They belong to the Crown, apparently.",
            Icon = "Assets/Items/gold_coins.png"
        };

        public static Item RoyalGoldCoin { get; } = new Item
        {
            Name = "Royal Gold Coin",
            Description = "A single gold coin, graciously granted by King Hautain to a loyal subject.",
            Icon = "Assets/Items/royal_gold_coin.png"
        };

        public static Item GargoyleBlessing { get; } = new Item
        {
            Name = "Gargoyle's Blessing",
            Description = "Not something you can hold, exactly. You just feel a little luckier. (Luck +1)",
            Icon = "Assets/Items/gargoyle_blessing.png"
        };

        private static readonly Dictionary<string, Item> ItemLookup =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { GoldCoins.Name, GoldCoins },
                { RoyalGoldCoin.Name, RoyalGoldCoin },
                { GargoyleBlessing.Name, GargoyleBlessing }
            };

        public static IEnumerable<Item> All => ItemLookup.Values;

        // Unknown names still work - they just get no icon/description.
        public static Item GetItem(string name)
        {
            return ItemLookup.TryGetValue(name, out Item? item)
                ? item
                : new Item { Name = name };
        }
    }
}
