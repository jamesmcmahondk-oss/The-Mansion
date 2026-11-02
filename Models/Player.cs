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

    }
}