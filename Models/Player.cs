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

        // ---- Helpers for scene/dialogue code -------------------------

        public bool Has(string itemName) => Inventory.Contains(itemName);

        // Adds the item (once - giving it again does nothing).
        public void Give(string itemName)
        {
            if (!Has(itemName))
            {
                Inventory.Add(itemName);
            }
        }

        public void Take(string itemName) => Inventory.Remove(itemName);

        public bool HasFlag(string flag) => Flags.Contains(flag);

        public void SetFlag(string flag) => Flags.Add(flag);

        public void ClearFlag(string flag) => Flags.Remove(flag);

    }
}