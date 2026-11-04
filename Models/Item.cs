using System;

namespace The_Mansion.Models
{
    /*
     * Something the player can carry. The player's inventory stores item
     * names (Player.Inventory); Data/Items.cs looks up the details here.
     */
    public class Item
    {
        // Must match the name added to Player.Inventory, e.g. "Gold Coins".
        public string Name { get; set; } = "";

        // Shown when the player hovers over the item in the inventory.
        public string Description { get; set; } = "";

        // 128x128 picture, e.g. "Assets/Items/gold_coins.png".
        // Leave empty to show the item's initials instead.
        public string Icon { get; set; } = "";

        // Added to the player's attack when this is their Weapon
        // (Player.Weapon = "Fork"). Having no weapon counts as -1.
        public int WeaponBonus { get; set; }

        // Optional: what happens when used with ITEM during a fight.
        // Return the text to show, e.g.
        //   (p, fight) => { fight.HurtEnemy(10); return "You throw it! (-10)"; }
        // Items without this "won't help in a fight".
        public Func<Player, Fight, string>? UseInFight { get; set; }

        // The item is gone after being used in a fight (e.g. thrown).
        public bool UsedUpInFight { get; set; }

        // A Memento: an object that stirs memories the player doesn't know
        // they have. Shown as "Memento" in the inventory.
        public bool IsMemento { get; set; }
    }

    /*
     * What happens when the player uses a particular item on a hotspot.
     * Added to a hotspot's ItemUses, keyed by item name.
     */
    public class ItemUse
    {
        // Text shown in the description panel.
        public string Text { get; set; } = "";

        // Optional: change stats, give/take items, etc.
        public Action<Player>? Effect { get; set; }

        // Take the item away after using it (e.g. it was given away).
        public bool RemoveItem { get; set; }

        // Optional: start a conversation (Data/Dialogues.cs) afterwards.
        public string DialogueId { get; set; } = "";

        // Optional: only react like this when true. A hotspot can have several
        // reactions to the same item; the first one whose condition is true
        // is used.
        public Func<Player, bool>? Condition { get; set; }
    }
}
