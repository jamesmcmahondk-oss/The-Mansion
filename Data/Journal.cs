using System;
using System.Collections.Generic;
using The_Mansion.Models;

namespace The_Mansion.Data
{
    /*
     * The player's journal. Entries write themselves: whenever something
     * happens, every entry whose condition has just become true is added
     * (in the order they were discovered) and "New journal entry" appears.
     *
     * To add one:  Entry("SomeId", "Title", "What the player noted.", p => p.HasFlag("SomeFlag"),
     *                    category: Puzzles, where: "The rear yard");
     * Pages: People (with picture: Portraits + "Name_portrait_framed.png"),
     * Puzzles (clues and where), Mementos (picture: the item's picture).
     * The id is what's saved, so don't rename it once players have saves.
     *
     * Keep entries in the player's voice, quote the game's own words, and
     * don't explain more than the player knows.
     */
    public static class Journal
    {
        public class JournalEntry
        {
            public string Id { get; init; } = "";
            public string Title { get; init; } = "";
            public string Text { get; init; } = "";
            public Func<Player, bool> When { get; init; } = _ => false;

            // Which page of the journal: People, Puzzles or Mementos.
            public string Category { get; init; } = People;

            // Optional picture on the right-hand page (a portrait, a memento).
            public string Picture { get; init; } = "";

            // Optional: where it was found / where the puzzle is.
            public string Where { get; init; } = "";
        }

        public const string People = "People";
        public const string Puzzles = "Puzzles";
        public const string Mementos = "Mementos";
        public static readonly string[] Categories = { People, Puzzles, Mementos };

        private const string Portraits = "Assets/Characters/Portraits/";

        private static readonly List<JournalEntry> AllEntries = new();
        private static readonly Dictionary<string, JournalEntry> EntryLookup =
            new(StringComparer.OrdinalIgnoreCase);

        private static JournalEntry Entry(string id, string title, string text, Func<Player, bool> when,
                                          string category = People, string picture = "", string where = "")
        {
            var entry = new JournalEntry
            {
                Id = id, Title = title, Text = text, When = when,
                Category = category, Picture = picture, Where = where
            };
            AllEntries.Add(entry);
            EntryLookup[id] = entry;
            return entry;
        }

        // =========================================================
        // PEOPLE
        // =========================================================

        public static JournalEntry Gargoyle { get; } =
            Entry("Gargoyle", "The gargoyle at the gate",
                  "A sulky, rather sad-looking gargoyle sits by the front gate. He talks.",
                  p => p.HasFlag("MetGargoyle"),
                  picture: Portraits + "Gargoyle_portrait_framed.png");

        public static JournalEntry GargoyleRain { get; } =
            Entry("GargoyleRain", "\"You don't cast rain properly\"",
                  "The gargoyle squinted at me through the drizzle and said: \"You don't cast rain properly.\" " +
                  "When I asked what he meant, he said \"Nothing\" - and looked away, very quickly.",
                  p => p.HasFlag("GargoyleNoticedRain"),
                  picture: Portraits + "Gargoyle_portrait_framed.png");

        public static JournalEntry Hautain { get; } =
            Entry("Hautain", "King Hautain",
                  "A frog with a crown rules the courtyard fountain from a lily pad: " +
                  "\"Hautain the First, Keeper of the Deep Basin, Lord of the Seven Lily Pads, Defender of the Western Drain.\" " +
                  "The gold in the fountain is the Royal Treasury.",
                  p => p.HasFlag("MetHautain"),
                  picture: Portraits + "Hautain_portrait_framed.png");

        public static JournalEntry Quazimodo { get; } =
            Entry("Quazimodo", "Quazimodo",
                  "The front door is kept by Quazimodo, a pale, crooked figure. One eye studies you; " +
                  "the other appears to be inspecting the weather.",
                  p => p.HasFlag("MetQuazimodo"),
                  picture: Portraits + "Quazimodo_portrait_framed.png");

        public static JournalEntry BenchNoise { get; } =
            Entry("BenchNoise", "Something under the bench",
                  "When I sat on the stone bench by the roses, something underneath it hiccupped. Then giggled.",
                  p => p.HasFlag("HeardBenchNoise"),
                  category: Puzzles, where: "Right side of the Mansion - the stone bench by the roses");

        public static JournalEntry Ebria { get; } =
            Entry("Ebria", "Ebria",
                  "A tiny fairy lives under the stone bench by the roses, a bottle clasped against her chest. Her name is Ebria.",
                  p => p.HasFlag("MetEbria"),
                  picture: Portraits + "Ebria_portrait_framed.png");

        public static JournalEntry EbriaMirror { get; } =
            Entry("EbriaMirror", "Ebria and the mirror",
                  "I showed Ebria the cracked mirror from the well. The drunken smile disappeared. \"No.\" " +
                  "She wouldn't look at it again.",
                  p => p.HasFlag("ShowedEbriaMirror"),
                  picture: Portraits + "Ebria_portrait_framed.png");

        // =========================================================
        // CLUES AND PLACES
        // =========================================================

        public static JournalEntry FrontDoor { get; } =
            Entry("FrontDoor", "The front door is open",
                  "The front door of the Mansion stands open. The house has admitted me.",
                  p => p.HasFlag("FrontDoorOpen"),
                  category: Puzzles, where: "The front door, at the top of the courtyard steps");

        public static JournalEntry StoleCoins { get; } =
            Entry("StoleCoins", "The Royal Treasury",
                  "I took a handful of gold coins from the fountain. They belong to the Crown, apparently.",
                  p => p.HasFlag("StoleCoins"),
                  category: Puzzles, where: "The courtyard fountain");

        public static JournalEntry HoodedStatue { get; } =
            Entry("HoodedStatue", "The hooded statue",
                  "A hooded stone figure in the garden, its hands resting on a great stone sword. " +
                  "There is no face under the hood - only shadow. Words are cut along the blade: " +
                  "HE WHO WOULD TAKE THE BLADE MUST FIRST SURRENDER HIS OWN.",
                  p => p.HasFlag("ReadStoneSword") || p.HasFlag("TookStoneSword"),
                  category: Puzzles, picture: "Assets/Backgrounds/Mansion/statue_closeup.png",
                  where: "The garden - the hooded statue on its plinth");

        public static JournalEntry FacelessKnight { get; } =
            Entry("FacelessKnight", "The Faceless Knight",
                  "The hooded statue in the garden held a stone sword. Words are cut along the blade: " +
                  "HE WHO WOULD TAKE THE BLADE MUST FIRST SURRENDER HIS OWN. " +
                  "Once I put my own weapon away, the sword came free.",
                  p => p.HasFlag("TookStoneSword"),
                  category: Puzzles, where: "The garden - the hooded statue");

        public static JournalEntry Glasshouse { get; } =
            Entry("Glasshouse", "The glasshouse",
                  "Hundreds of candles burn inside the glasshouse. Someone lit every one. " +
                  "The doors won't open, yet they rattled as if something inside pushed back.",
                  p => p.HasFlag("TriedGlasshouseDoors"),
                  category: Puzzles, where: "The garden - the glasshouse");

        public static JournalEntry RoseBush { get; } =
            Entry("RoseBush", "The roses",
                  "The roses by the bench have thorns. I have conducted this experiment.",
                  p => p.HasFlag("PrickedByRoseBush"),
                  category: Puzzles, where: "Right side of the Mansion - the rose bush");

        public static JournalEntry KitchenDoor { get; } =
            Entry("KitchenDoor", "The kitchen door",
                  "The service door in the rear yard is bolted from the other side.",
                  p => p.HasFlag("TriedKitchenDoor"),
                  category: Puzzles, where: "The rear yard - the service door");

        public static JournalEntry CellarDoors { get; } =
            Entry("CellarDoors", "The cellar doors",
                  "Something underneath the cellar doors in the rear yard holds them shut. " +
                  "When I pulled the ring, a single quiet sound came from underneath. Knock.",
                  p => p.HasFlag("TriedCellarDoor"),
                  category: Puzzles, where: "The rear yard - the cellar doors");

        public static JournalEntry WellMirror { get; } =
            Entry("WellMirror", "The mirror in the well",
                  "I fished a small silver-backed hand mirror from the bottom of the well. The glass is clouded and cracked.",
                  p => p.HasFlag("GotWellMirror"),
                  category: Puzzles, where: "The old well, past the right side of the Mansion");

        public static JournalEntry Gravestone { get; } =
            Entry("Gravestone", "The gravestone",
                  "The inscription on the old gravestone by the graveyard gate reads: " +
                  "THRICE FOR HEAVEN · ONCE FOR THE HEART · NEVER FOR THE HAND THAT LET GO",
                  p => p.HasFlag("ReadGravestone"),
                  category: Puzzles, where: "The graveyard - the old gravestone by the gate");

        public static JournalEntry Photograph { get; } =
            Entry("Photograph", "The photograph",
                  "Hidden in the plinth of the stone cross: a tiny, old photograph of the Mansion as it once was. " +
                  "One face has decayed away. On the back: \"You said you'd come back.\" " +
                  "When I picked it up, the rain stopped, and I remembered sunlight. I don't remember remembering sunlight.",
                  p => p.HasFlag("FoundPhotograph"),
                  category: Mementos, picture: "Assets/Items/Sources/worn_photograph.png", where: "The graveyard - hidden in the plinth of the stone cross");

        // =========================================================
        // LOOKUP (used by the game - no need to edit)
        // =========================================================

        public static IReadOnlyList<JournalEntry> All => AllEntries;

        public static JournalEntry? Get(string id) =>
            EntryLookup.TryGetValue(id, out JournalEntry? entry) ? entry : null;

        // Adds every entry that has just come true. Returns the new ones.
        public static List<JournalEntry> Update(Player player)
        {
            var added = new List<JournalEntry>();

            foreach (JournalEntry entry in AllEntries)
            {
                if (!player.Journal.Contains(entry.Id) && entry.When(player))
                {
                    player.Journal.Add(entry.Id);
                    added.Add(entry);
                }
            }

            return added;
        }
    }
}
