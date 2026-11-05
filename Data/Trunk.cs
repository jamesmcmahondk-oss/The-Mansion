using System;
using System.Collections.Generic;
using The_Mansion.Models;

namespace The_Mansion.Data
{
    /*
     * The pre-game "car trunk" screen: after rolling a character, the
     * player picks ONE of these to take into the Mansion.
     *
     * Each line: item name (must exist in Data/Items.cs), then where it is
     * painted in Assets/UI/trunk.png - X, Y, width, height in that image's
     * pixels (0-1536 across, 0-1024 down; Y 80-944 is visible on screen).
     * Optional onTake runs when the player takes it.
     * Where boxes overlap, the one listed LAST is on top.
     */
    public static class Trunk
    {
        public static List<TrunkItem> Items { get; } = new()
        {
            Add("Corkscrew",      375, 440, 210, 165),
            Add("Lighter",        270, 570, 175, 140),
            Add("Ball of String", 630, 505, 160, 165),
            Add("Party Hat",      845, 325, 250, 205),
            Add("Loose Change",   850, 575, 180,  95),
            Add("Empty Bottle",  1180, 395, 290, 310),

            // Listed after the bottle so it wins where their boxes overlap.
            Add("Fork",          1050, 505, 210, 240,
                onTake: p => p.Weapon = "Fork"),
        };

        private static TrunkItem Add(
            string itemName,
            double x, double y, double width, double height,
            Action<Player>? onTake = null)
        {
            return new TrunkItem
            {
                ItemName = itemName,
                X = x,
                Y = y,
                Width = width,
                Height = height,
                OnTake = onTake
            };
        }
    }

    public class TrunkItem
    {
        public string ItemName { get; set; } = "";
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Action<Player>? OnTake { get; set; }
    }
}
