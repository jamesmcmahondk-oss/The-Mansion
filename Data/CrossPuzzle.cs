using System;
using The_Mansion.Models;

namespace The_Mansion.Data
{
    /*
     * The Graveyard Secret: three stone spirals at the centre of the cross.
     * Each turns a quarter at a time. The gravestone says how:
     *   THRICE FOR HEAVEN (up spiral 3 turns) · ONCE FOR THE HEART (left, 1)
     *   NEVER FOR THE HAND THAT LET GO (right, 0)
     * Turns are counted forwards from where each spiral starts (0-3), and are
     * saved as flags (SpiralUpper1..3 etc.). Solving it sets CrossOpened.
     * The puzzle screen is MainWindow.CrossPuzzle.cs.
     */
    public static class CrossPuzzle
    {
        public static readonly string[] Spirals = { "Upper", "Left", "Right" };

        // Which way each spiral's tail points before it is turned
        // (0 up, 1 right, 2 down, 3 left).
        private static int StartDirection(string spiral) => spiral switch
        {
            "Upper" => 0,
            "Right" => 1,
            _ => 3
        };

        public static bool CanTurn(Player p) => p.HasFlag("ReadGravestone") && !p.HasFlag("CrossOpened");

        public static int Turns(Player p, string spiral)
        {
            for (int turns = 1; turns < 4; turns++)
            {
                if (p.HasFlag($"Spiral{spiral}{turns}"))
                {
                    return turns;
                }
            }
            return 0;
        }

        // The tail's direction in degrees, clockwise from up.
        public static double Angle(Player p, string spiral) =>
            (StartDirection(spiral) + Turns(p, spiral)) % 4 * 90;

        // Turns one spiral a quarter (step +1 clockwise, -1 back).
        // Returns true if this solves the puzzle.
        public static bool Turn(Player p, string spiral, int step)
        {
            int turns = ((Turns(p, spiral) + step) % 4 + 4) % 4;

            for (int t = 1; t < 4; t++)
            {
                p.ClearFlag($"Spiral{spiral}{t}");
            }
            if (turns > 0)
            {
                p.SetFlag($"Spiral{spiral}{turns}");
            }

            if (Turns(p, "Upper") == 3 && Turns(p, "Left") == 1 && Turns(p, "Right") == 0)
            {
                p.SetFlag("CrossOpened");
                return true;
            }
            return false;
        }
    }
}
