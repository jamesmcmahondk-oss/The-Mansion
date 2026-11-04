using System;
using System.Collections.Generic;

namespace The_Mansion.Data
{
    /*
     * The fallback replies when a verb does nothing. Each has a few
     * variants so the game doesn't repeat itself; the same line never
     * comes up twice in a row.
     *
     * Important objects should get their own lines instead:
     *   .OnTalk("...") / .OnAct("...")   on the hotspot (wrong verb)
     *   .OnOtherItems("... {item} ...")  on the hotspot (wrong item)
     *   misuse: "... {target} ..."       on the item in Data/Items.cs
     */
    public static class Replies
    {
        private static readonly string[] NothingToSee =
        {
            "You see nothing special.",
            "Nothing about it catches your eye.",
            "It's exactly what it looks like.",
            "You look closely. Nothing unusual."
        };

        private static readonly string[] CantTalk =
        {
            "Who or what are you talking to?",
            "It doesn't answer.",
            "You get no reply. You expected none.",
            "Talking to it achieves very little."
        };

        private static readonly string[] CantAct =
        {
            "You can't do that.",
            "That doesn't seem to do anything.",
            "Nothing happens.",
            "You'd rather not.",
            "That won't help."
        };

        // {item} and {target} are filled in.
        private static readonly string[] CantUse =
        {
            "You can't use the {item} on the {target}.",
            "The {item} doesn't work on the {target}.",
            "Using the {item} on the {target} achieves nothing.",
            "That's not what the {item} is for."
        };

        public static string Nothing() => Pick(NothingToSee);

        public static string NoTalk() => Pick(CantTalk);

        public static string NoAct() => Pick(CantAct);

        public static string NoUse(string item, string target) =>
            Fill(Pick(CantUse), item, target);

        // "{item}"/"{target}" -> the names (targets are lower-case after "the").
        public static string Fill(string text, string item, string target) =>
            text.Replace("{item}", item).Replace("{target}", target);

        private static readonly Random Random = new();
        private static readonly Dictionary<string[], int> LastPicked = new();

        private static string Pick(string[] variants)
        {
            int last = LastPicked.TryGetValue(variants, out int index) ? index : -1;
            int next = Random.Next(variants.Length);

            if (next == last && variants.Length > 1)
            {
                next = (next + 1 + Random.Next(variants.Length - 1)) % variants.Length;
            }

            LastPicked[variants] = next;
            return variants[next];
        }
    }
}
