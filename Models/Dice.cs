using System;

namespace The_Mansion.Models
{
    /*
     * Dice for fights and stat tests.
     */
    public static class Dice
    {
        private static readonly Random Random = new();

        // One six-sided die: 1-6.
        public static int D6() => Random.Next(1, 7);

        // Several dice added together, e.g. Roll(3) = 3d6 (3-18).
        public static int Roll(int count)
        {
            int total = 0;
            for (int i = 0; i < count; i++)
            {
                total += D6();
            }
            return total;
        }
    }
}
