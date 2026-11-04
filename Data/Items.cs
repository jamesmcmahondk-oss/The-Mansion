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

        /*
         * Optional extras:
         *   weaponBonus  - added to attacks when this is the player's Weapon
         *   inFight      - what using it in a fight does; return the text to show
         *   usedUpInFight - it's gone after being used in a fight
         */
        private static Item NewItem(
            string name,
            string description,
            string icon = "",
            int weaponBonus = 0,
            Func<Player, Fight, string>? inFight = null,
            bool usedUpInFight = false,
            bool memento = false)
        {
            var item = new Item
            {
                Name = name,
                Description = description,
                Icon = icon,
                WeaponBonus = weaponBonus,
                UseInFight = inFight,
                UsedUpInFight = usedUpInFight,
                IsMemento = memento
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
        // OUTSIDE THE MANSION (see PUZZLES.md)
        // =========================================================

        // The Faceless Knight's sword (statue close-up).
        public static Item StoneSword { get; } =
            NewItem("Stone Sword",
                    "The hooded statue's sword. It looks like solid stone and weighs almost nothing. (Weapon: +2 in fights.)",
                    weaponBonus: 2);

        // From the bottom of the well. Not Ebria's - but she won't look at it.
        public static Item CrackedHandMirror { get; } =
            NewItem("Cracked Hand Mirror",
                    "A small silver-backed hand mirror, fished from the bottom of the well. The glass is cracked straight across.");

        // The first Memento: from the hidden compartment in the cross plinth.
        public static Item WornPhotograph { get; } =
            NewItem("Worn Photograph",
                    "A tiny, old photograph of the Mansion as it once was. A small group of people stands in front of it. " +
                    "Where one face should be, the picture has simply decayed away. On the back, in faded handwriting: " +
                    "\"You said you'd come back.\"",
                    memento: true);

        // For a rubbing of the gravestone (use either one on the inscription
        // while carrying both). Where they're found is still to be decided.
        public static Item Charcoal { get; } =
            NewItem("Charcoal",
                    "A stick of charcoal. It leaves black smudges on everything, mostly you.");

        public static Item Paper { get; } =
            NewItem("Paper",
                    "A few sheets of thin, yellowed paper.");

        // Found in the tool shed (not built yet). Cuts a rose safely.
        public static Item GardenShears { get; } =
            NewItem("Garden Shears",
                    "Rusty garden shears. They still cut - mostly.");

        public static Item RedRose { get; } =
            NewItem("Red Rose",
                    "One perfect dark-red rose. It smells of summer, which is wrong this late in the year.");

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
                    "A silver fork. Technically a weapon. (Taking it makes it your weapon: +1 in fights.)",
                    "Assets/Items/fork.png",
                    weaponBonus: 1);

        public static Item EmptyBottle { get; } =
            NewItem("Empty Bottle",
                    "An empty glass bottle. It could hold something. Or be thrown at something.",
                    "Assets/Items/empty_bottle.png",
                    inFight: (p, fight) =>
                    {
                        fight.HurtEnemy(10);
                        return $"You hurl the bottle! It shatters over {fight.Enemy.Name}. (-10)";
                    },
                    usedUpInFight: true);


        // =========================================================
        // LOOKUP (used by the game - no need to edit)
        // =========================================================

        public static IEnumerable<Item> All => ItemLookup.Values;

        public static bool Exists(string name) => ItemLookup.ContainsKey(name);

        // Unknown names still work - they just get no icon/description.
        public static Item GetItem(string name)
        {
            return ItemLookup.TryGetValue(name, out Item? item)
                ? item
                : new Item { Name = name };
        }
    }
}
