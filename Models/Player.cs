using System;
using System.Collections.Generic;
using System.Text;

namespace The_Mansion.Models
{
    public class Player
    {
        public string Name { get; set; } = "";

        public int Health { get; set; }

        public int MaxHealth { get; set; }

        public int Strength { get; set; }

        public int Luck { get; set; }

        public int Wits { get; set; }

        public string Weapon { get; set; } = "None";

        public string StatusEffect { get; set; } = "None";
        public List<string> Inventory { get; set; } = new();

        // Story flags: things that have happened, e.g. "StoleCoins".
        // Use them to change what scenes, hotspots and characters do.
        public HashSet<string> Flags { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        // Conversation lines already heard, as "DialogueId:lineId" - choices
        // leading to them are shown dimmed, so new topics stand out.
        public HashSet<string> SeenLines { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        // Journal entries found so far (ids from Data/Journal.cs), oldest first.
        public List<string> Journal { get; set; } = new();

        // ---- Helpers for scene/dialogue code -------------------------

        // Item names ignore upper/lower case, like scene names and flags.
        public bool Has(string itemName) => Inventory.Contains(itemName, StringComparer.OrdinalIgnoreCase);

        // Adds the item (once - giving it again does nothing).
        public void Give(string itemName)
        {
            if (!Has(itemName))
            {
                Inventory.Add(itemName);
            }
        }

        // Removes the item. If it was the weapon in hand, the hand is empty now.
        public void Take(string itemName)
        {
            int index = Inventory.FindIndex(name => string.Equals(name, itemName, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                Inventory.RemoveAt(index);
            }

            if (string.Equals(Weapon, itemName, StringComparison.OrdinalIgnoreCase) && !Has(itemName))
            {
                Weapon = "None";
            }
        }

        public bool HasFlag(string flag) => Flags.Contains(flag);

        public void SetFlag(string flag) => Flags.Add(flag);

        public void ClearFlag(string flag) => Flags.Remove(flag);

        // ---- Stat requirements (for scenes and puzzles) ----------------
        // No dice: a task needs a fixed Wits / Luck / Strength. Either the
        // hero has it or not - and the game says so (see .NeedsStat).

        public int Stat(string stat) => stat switch
        {
            "Wits" => Wits,
            "Luck" => Luck,
            "Strength" => Strength,
            _ => 0
        };

        public bool Meets(string stat, int needed) => Stat(stat) >= needed;

    }
}