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
         *   misuse       - its own reply when used on something that doesn't
         *                  react to it; {target} is the thing, e.g.
         *                  "There's no cork in the {target}."
         */
        private static Item NewItem(
            string name,
            string description,
            string icon = "",
            int weaponBonus = 0,
            Func<Player, Fight, string>? inFight = null,
            bool usedUpInFight = false,
            bool memento = false,
            string picture = "",
            string misuse = "",
            string reads = "")
        {
            var item = new Item
            {
                Name = name,
                Description = description,
                Icon = icon,
                WeaponBonus = weaponBonus,
                UseInFight = inFight,
                UsedUpInFight = usedUpInFight,
                IsMemento = memento,
                Picture = picture,
                MisuseText = misuse,
                ReadsDialogue = reads
            };

            ItemLookup[name] = item;
            return item;
        }

        public static Item GoldCoins { get; } =
            NewItem("Gold Coins",
                    "A dripping handful of gold coins from the fountain. They belong to the Crown, apparently. (Right-click to look at it.)",
                    "Assets/Items/gold_coins.png",
                    picture: "Assets/Items/Sources/gold_coins.png",
                    misuse: "The {target} shows no interest in money.");

        public static Item RoyalGoldCoin { get; } =
            NewItem("Royal Gold Coin",
                    "A single gold coin, graciously granted by King Hautain to a loyal subject. (Right-click to look at it.)",
                    "Assets/Items/royal_gold_coin.png",
                    picture: "Assets/Items/Sources/royal_gold_coin.png",
                    misuse: "The {target} doesn't recognise the King's coin. Or the King.");

        public static Item GargoyleBlessing { get; } =
            NewItem("Gargoyle's Blessing",
                    "Not something you can hold, exactly. You just feel a little luckier. (Luck +1)",
                    "Assets/Items/gargoyle_blessing.png");


        // =========================================================
        // OUTSIDE THE MANSION (see PUZZLES.md)
        // =========================================================

        // The player starts with it (and holding it) - which is why the
        // Faceless Knight's stone fingers tighten until it's put away.
        public static Item PocketKnife { get; } =
            NewItem("Pocket Knife",
                    "A small folding knife with a worn red handle. You've had it for as long as you can remember - which, admittedly, isn't long. (Weapon. Right-click to look at it.)",
                    "Assets/Items/pocket_knife.png",
                    weaponBonus: 0,
                    picture: "Assets/Items/Sources/pocket_knife.png",
                    misuse: "You'd rather not ruin your knife on the {target}.");

        // The Faceless Knight's sword (statue close-up).
        public static Item StoneSword { get; } =
            NewItem("Stone Sword",
                    "The hooded statue's sword. It looks like solid stone and weighs almost nothing. (Weapon: +2 in fights.) (Right-click to look at it.)",
                    "Assets/Items/stone_sword.png",
                    picture: "Assets/Items/Sources/stone_sword.png",
                    weaponBonus: 2,
                    misuse: "You'd rather not chip the stone sword on the {target}.");

        // From the bottom of the well. Not Ebria's - but she won't look at it.
        public static Item CrackedHandMirror { get; } =
            NewItem("Cracked Hand Mirror",
                    "A small silver-backed hand mirror, fished from the bottom of the well. The glass is clouded and cracked. (Right-click to look at it.)",
                    "Assets/Items/cracked_hand_mirror.png",
                    picture: "Assets/Items/Sources/cracked_hand_mirror.png",
                    misuse: "You angle the mirror at the {target}. The crack runs straight through the reflection.");

        // The first Memento: from the hidden compartment in the cross plinth.
        public static Item WornPhotograph { get; } =
            NewItem("Worn Photograph",
                    "A tiny, old photograph of the Mansion as it once was. A small group of people stands in front of it. " +
                    "Where one face should be, the picture has simply decayed away. On the back, in faded handwriting: " +
                    "\"You said you'd come back.\" (Right-click to look at it.)",
                    "Assets/Items/worn_photograph.png",
                    memento: true,
                    picture: "Assets/Items/Sources/worn_photograph.png",
                    misuse: "You hold the photograph up beside the {target}. It doesn't help you remember anything.");

        // From the hidden compartment behind the empty frame in the Portrait
        // Gallery. Right-click reads it (Dialogues.PortraitLedger).
        public static Item PortraitLedger { get; } =
            NewItem("Portrait Ledger",
                    "A handwritten record of portraits commissioned for the Mansion. Several pages have been removed. " +
                    "Several names have been crossed out. One appears to have been written repeatedly. (Right-click to read it.)",
                    "Assets/Items/portrait_ledger.png",
                    reads: "PortraitLedger",
                    misuse: "You leaf through the ledger beside the {target}. It isn't listed.");

        // For a rubbing of the gravestone (use either one on the inscription
        // while carrying both). Charcoal: the tool shed. Paper: still to be decided.
        public static Item Charcoal { get; } =
            NewItem("Charcoal",
                    "A stick of charcoal. It leaves black smudges on everything, mostly you. (Right-click to look at it.)",
                    "Assets/Items/charcoal.png",
                    picture: "Assets/Items/Sources/charcoal.png",
                    misuse: "You'd only make a mess of the {target}. And of yourself.");

        public static Item Paper { get; } =
            NewItem("Paper",
                    "A few sheets of thin, yellowed paper. (Right-click to look at it.)",
                    "Assets/Items/paper.png",
                    picture: "Assets/Items/Sources/paper.png",
                    misuse: "There's nothing on the {target} worth writing down.");

        // Found in the tool shed (rear yard). Cuts a rose safely.
        public static Item GardenShears { get; } =
            NewItem("Garden Shears",
                    "Rusty garden shears. They still cut - mostly. (Right-click to look at it.)",
                    "Assets/Items/garden_shears.png",
                    picture: "Assets/Items/Sources/garden_shears.png",
                    misuse: "There's nothing on the {target} that needs cutting.");

        // From the tool shed wall (rear yard). Use still to be decided.
        public static Item Rope { get; } =
            NewItem("Rope",
                    "A coil of old rope. Damp, but stronger than it looks. (Right-click to look at it.)",
                    "Assets/Items/rope.png",
                    picture: "Assets/Items/Sources/rope.png",
                    misuse: "You can't think of a useful way to tie the rope to the {target}.");

        public static Item RedRose { get; } =
            NewItem("Red Rose",
                    "One perfect dark-red rose. It smells of summer, which is wrong this late in the year. (Right-click to look at it.)",
                    "Assets/Items/red_rose.png",
                    picture: "Assets/Items/Sources/red_rose.png",
                    misuse: "The {target} doesn't seem like the place for the rose.");

        // =========================================================
        // THE CAR TRUNK (pick one before the game starts - Data/Trunk.cs)
        // =========================================================

        public static Item Corkscrew { get; } =
            NewItem("Corkscrew",
                    "A wooden-handled corkscrew. Opens bottles. Possibly other things, if you're creative. (Right-click to look at it.)",
                    "Assets/Items/corkscrew.png",
                    picture: "Assets/Items/Sources/corkscrew.png",
                    misuse: "There's no cork in the {target}.");

        public static Item Lighter { get; } =
            NewItem("Lighter",
                    "A battered petrol lighter. It still sparks - just. Light in a dark place is never wasted.",
                    "Assets/Items/lighter.png",
                    misuse: "The lighter sparks. The {target} is far too wet to care.");

        public static Item BallOfString { get; } =
            NewItem("Ball of String",
                    "A tangle of rough twine. Good for tying, climbing, or pretending you have a plan.",
                    "Assets/Items/ball_of_string.png",
                    misuse: "You tie the string to the {target}, consider your plan, and untie it again.");

        public static Item PartyHat { get; } =
            NewItem("Party Hat",
                    "A polka-dot party hat trimmed with red tinsel. Nobody can stay angry at someone wearing a party hat. Probably.",
                    "Assets/Items/party_hat.png",
                    misuse: "The {target} is not in a party mood.");

        public static Item LooseChange { get; } =
            NewItem("Loose Change",
                    "A few coins from the cup holder. Not much, but money talks - even in haunted mansions. (Right-click to look at it.)",
                    "Assets/Items/loose_change.png",
                    picture: "Assets/Items/Sources/loose_change.png",
                    misuse: "The {target} can't be bought. Not with that much, anyway.");

        public static Item Fork { get; } =
            NewItem("Fork",
                    "A silver fork. Technically a weapon. (Taking it makes it your weapon: +1 in fights.) (Right-click to look at it.)",
                    "Assets/Items/fork.png",
                    picture: "Assets/Items/Sources/fork.png",
                    weaponBonus: 1,
                    misuse: "You prod the {target} with the fork. Nothing comes of it.");

        public static Item EmptyBottle { get; } =
            NewItem("Empty Bottle",
                    "An empty glass bottle. It could hold something. Or be thrown at something. (Right-click to look at it.)",
                    "Assets/Items/empty_bottle.png",
                    picture: "Assets/Items/Sources/empty_bottle.png",
                    inFight: (p, fight) =>
                    {
                        fight.HurtEnemy(10);
                        return $"You hurl the bottle! It shatters over {fight.Enemy.Name}. (-10)";
                    },
                    usedUpInFight: true,
                    misuse: "You hold the bottle up to the {target}. It stays empty.");


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
