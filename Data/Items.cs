using System;
using System.Collections.Generic;
using The_Mansion.Models;

namespace The_Mansion.Data
{
    /*
     * Every item in the game: name, description (shown when hovered in the
     * inventory) and a 128x128 icon (built by Tools/build_item_icons.py).
     *
     * Give an item to the player with  p.Give("Item Name")  - for example in
     * a dialogue choice's effect or a hotspot's OnAct.
     */
    public static class Items
    {
        // Must stay at the top: items below add themselves to it.
        private static readonly Dictionary<string, Item> ItemLookup =
            new(StringComparer.OrdinalIgnoreCase);

        private static Item NewItem(string name, string description, string icon = "")
        {
            var item = new Item
            {
                Name = name,
                Description = description,
                Icon = icon
            };

            ItemLookup[name] = item;
            return item;
        }

        public static Item GoldCoins { get; } =
            NewItem("Gold Coins",
                    "A dripping handful of gold coins from the fountain. They belong to the Crown, apparently.",
                    "Assets/Items/gold_coins.png");

        public static Item RoyalGoldCoin { get; } =
            NewItem("Royal Gold Coin",
                    "A single gold coin, graciously granted by King Hautain to a loyal subject.",
                    "Assets/Items/royal_gold_coin.png");

        public static Item GargoyleBlessing { get; } =
            NewItem("Gargoyle's Blessing",
                    "Not something you can hold, exactly. You just feel a little luckier. (Luck +1)",
                    "Assets/Items/gargoyle_blessing.png");


        // =========================================================
        // THE CAR TRUNK (pick one before the game starts - Data/Trunk.cs)
        // =========================================================

        public static Item Corkscrew { get; } =
            NewItem("Corkscrew",
                    "A wooden-handled corkscrew. Opens bottles. Possibly other things, if you're creative.",
                    "Assets/Items/corkscrew.png");

        public static Item Lighter { get; } =
            NewItem("Lighter",
                    "A battered petrol lighter. It still sparks - just. Light in a dark place is never wasted.",
                    "Assets/Items/lighter.png");

        public static Item BallOfString { get; } =
            NewItem("Ball of String",
                    "A tangle of rough twine. Good for tying, climbing, or pretending you have a plan.",
                    "Assets/Items/ball_of_string.png");

        public static Item PartyHat { get; } =
            NewItem("Party Hat",
                    "A polka-dot party hat trimmed with red tinsel. Nobody can stay angry at someone wearing a party hat. Probably.",
                    "Assets/Items/party_hat.png");

        public static Item LooseChange { get; } =
            NewItem("Loose Change",
                    "A few coins from the cup holder. Not much, but money talks - even in haunted mansions.",
                    "Assets/Items/loose_change.png");

        public static Item Fork { get; } =
            NewItem("Fork",
                    "A silver fork. Technically a weapon. (Taking it makes it your weapon.)",
                    "Assets/Items/fork.png");

        public static Item EmptyBottle { get; } =
            NewItem("Empty Bottle",
                    "An empty glass bottle. It could hold something. Or be thrown at something.",
                    "Assets/Items/empty_bottle.png");


        // =========================================================
        // LOOKUP (used by the game - no need to edit)
        // =========================================================

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
