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
    }
}
