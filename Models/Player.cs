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

        // Journal entries found so far (ids from Data/Journal.cs), oldest first.
        public List<string> Journal { get; set; } = new();

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

        // ---- Luck and Wits tests (for scenes and puzzles) -------------
        // Roll 3d6: equal to or under the stat succeeds.
        // Luck/Wits 10 = 50%, 12 = 74%, 7 = 16%.

        // The dice total of the last test (if you want to show it).
        public int LastTestRoll { get; private set; }

        public bool TestLuck() => Test(Luck);

        public bool TestWits() => Test(Wits);

        private bool Test(int stat)
        {
            LastTestRoll = Dice.Roll(3);
            return LastTestRoll <= stat;
        }

    }
}