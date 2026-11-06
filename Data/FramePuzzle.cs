using System;
using The_Mansion.Models;

namespace The_Mansion.Data
{
    /*
     * The Missing Portrait: three brass medallions on the empty frame in the
     * Portrait Gallery, each engraved with Mirélle's profile at one age
     * (left: middle age, centre: young, right: old - as her portraits hang,
     * dated 1821, 1798, 1854). Pressed in the order she was painted -
     * centre, left, right - they release the frame. A wrong press pops them
     * all back up; after three wrong tries, a hint. Progress is saved as
     * flags (PortraitSequenceStep1/2). The screen is MainWindow.FramePuzzle.cs.
     */
    public static class FramePuzzle
    {
        public static readonly string[] Medallions = { "Left", "Centre", "Right" };

        // Youngest to oldest.
        private static readonly string[] Order = { "Centre", "Left", "Right" };

        public enum Result { Pressed, Wrong, Solved }

        public static int Step(Player p) =>
            p.HasFlag("PortraitSequenceStep2") ? 2 : p.HasFlag("PortraitSequenceStep1") ? 1 : 0;

        // Is this medallion pressed down right now?
        public static bool IsPressed(Player p, string medallion) =>
            Array.IndexOf(Order, medallion) < Step(p);

        public static bool ShowsHint(Player p) => p.HasFlag("PortraitWrong3");

        private static void ResetSequence(Player p)
        {
            p.ClearFlag("PortraitSequenceStep1");
            p.ClearFlag("PortraitSequenceStep2");
        }

        public static Result Press(Player p, string medallion, out string text)
        {
            int step = Step(p);

            if (Order[step] != medallion)
            {
                ResetSequence(p);
                string counter = p.HasFlag("PortraitWrong2") ? "PortraitWrong3"
                               : p.HasFlag("PortraitWrong1") ? "PortraitWrong2"
                               : "PortraitWrong1";
                p.SetFlag(counter);

                text = "Click. ... The three medallions return to their original positions.";
                if (ShowsHint(p))
                {
                    text += " The portraits are not three members of a family. They are one life, viewed at different times.";
                }
                return Result.Wrong;
            }

            if (step == 2)
            {
                ResetSequence(p);
                p.SetFlag("SolvedPortraitGalleryPuzzle");
                text = "Click. ... Something releases inside the wall.";
                return Result.Solved;
            }

            p.SetFlag($"PortraitSequenceStep{step + 1}");
            text = step == 0 ? "Click. The medallion stays down." : "Click. The second medallion stays down too.";
            return Result.Pressed;
        }
    }
}
