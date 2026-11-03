using System;
using System.Collections.Generic;
using The_Mansion.Models;

namespace The_Mansion.Data
{
    /*
     * All scenes in the game. See SCENE_GUIDE.md for how to write them.
     *
     * Hotspot positions are in pixels of the scene's background image
     * (0-1536 across, 0-1024 down; only Y 192-832 is visible in game).
     * Measure them with the F1 debug menu's hotspot editor.
     *
     * Scenes register themselves - just add a new one below.
     */
    public static class Scenes
    {
        // Must stay at the top: scenes below add themselves to it.
        private static readonly Dictionary<string, Scene> SceneLookup =
            new(StringComparer.OrdinalIgnoreCase);

        private static SceneBuilder NewScene(string name, string backgroundImage, string description)
            => new SceneBuilder(name, backgroundImage, description);

        // =========================================================
        // OUTSIDE THE MANSION
        // =========================================================

        public static Scene FrontGate { get; } =
            NewScene("FrontGate",
                     "Assets/Backgrounds/Mansion/frontgate.png",
                     "You are standing before an old and creepy mansion.")

            .Hotspot("Fountain", 615, 545, 270, 205)
                .Describe("An ancient stone fountain stands in the courtyard beyond the gates.")
                .GoesTo("Courtyard")

            .Hotspot("Front Door", 905, 505, 95, 140)
                .Describe("The enormous wooden door looks like it has not opened in years.")
                .GoesTo("FrontDoor")

            .Hotspot("Gargoyle", 135, 205, 205, 165)
                .Describe("A sulky and rather sad looking gargoyle sits here.")
                .GoesTo("FrontGargoyle");


        public static Scene FountainCloseup { get; } =
            NewScene("FountainCloseup",
                     "Assets/Backgrounds/Mansion/fountain_closeup.png",
                     "Lily pads float on the dark water. Beneath the surface, a small pile of gold coins glitters.")

            .Hotspot("Gold Coins", 595, 655, 295, 175)
                .Describe("Several old gold coins rest at the bottom of the fountain.")

                // ACT on the coins: King Hautain appears on the lily pad.
                .ActStarts("HautainCoins")

                // Put the stolen coins back to make peace with the King.
                .OnItem("Gold Coins",
                        "You drop the stolen coins back into the fountain. From somewhere among the lily pads comes a grudging \"Hmph.\" (Royal Grudge lifted)",
                        removeItem: true,
                        effect: p =>
                        {
                            p.ClearFlag("StoleCoins");
                            p.StatusEffect = "None";
                        })

            .ReturnTo("Courtyard", "You step away from the fountain.");


        // Between the front gate and the fountain close-up / front door.
        // Background made by Tools/build_courtyard.py.
        public static Scene Courtyard { get; } =
            NewScene("Courtyard",
                     "Assets/Backgrounds/Mansion/courtyard.png",
                     "Rain-slick cobblestones glitter in the lantern light. A tiered fountain splashes beside the steps up to the front door.")

            .Hotspot("Fountain", 0, 300, 380, 470)
                .Describe("Water spills over the fountain's mossy tiers. Something glitters beneath the surface.")
                .GoesTo("FountainCloseup")

            .Hotspot("Front Door", 770, 280, 300, 410)
                .Describe("Wide stone steps lead up to the mansion's great carved doors.")
                .GoesTo("FrontDoor")

            .Hotspot("Garden Path", 400, 560, 220, 110)
                .Describe("A path runs behind the fountain, along the balustrade, around the left wing of the mansion.")
                .GoesTo("Garden")

            .Hotspot("Side Path", 1300, 540, 200, 200)
                .Describe("A cracked, winding path disappears into the overgrown dark beside the mansion.")

            .ReturnTo("FrontGate", "You walk back to the gates.");


        // Left of the courtyard: the overgrown garden with the glasshouse.
        // Background made by Tools/build_wide_backgrounds.py.
        public static Scene Garden { get; } =
            NewScene("Garden",
                     "Assets/Backgrounds/Mansion/garden.png",
                     "The path ends in an overgrown garden. Mist curls around a hooded statue, and beyond it a great glasshouse glows in the dark.")

            .Hotspot("Glasshouse", 470, 250, 530, 270)
                .Describe("A vast glasshouse of iron and glass. Candlelight flickers inside, though nobody seems to be tending it.")

            .Hotspot("Mansion", 1120, 192, 416, 300)
                .Describe("Ivy smothers the mansion's left wing. Warm light glows in the tall windows.")

            // Listed after the glasshouse so it wins where they overlap.
            .Hotspot("Statue", 770, 330, 120, 270)
                .Describe("A hooded stone figure stands on a plinth in the long grass, watching the path.")
                .GoesTo("StatueCloseup")

            .ReturnTo("Courtyard", "You follow the path back to the courtyard.");


        public static Scene StatueCloseup { get; } =
            NewScene("StatueCloseup",
                     "Assets/Backgrounds/Mansion/statue_closeup.png",
                     "The statue towers over you: a hooded figure in a ragged stone robe, its hands resting on the hilt of a great sword.")

            .Hotspot("Glasshouse", 264, 330, 384, 320)
                .Describe("Through the mist, candles glow behind the glasshouse's tall panes.")

            .Hotspot("Glasshouse", 992, 330, 352, 320)
                .Describe("Through the mist, candles glow behind the glasshouse's tall panes.")

            .Hotspot("Statue", 656, 200, 336, 624)
                .Describe("Ivy creeps up the ragged stone robe. Under the hood there is only darkness.")
                .OnTalk("The statue says nothing. Statues usually don't. Usually.")

            // Listed after the statue so they win where they overlap.
            .Hotspot("Hood", 808, 205, 72, 105)
                .Describe("You peer into the hood. There is no face carved there at all - just shadow.")

            .Hotspot("Stone Sword", 825, 350, 55, 425)
                .Describe("A great stone sword, its point resting on the plinth. The hilt is worn smooth, as if many hands have gripped it.")
                .OnAct("You pull at the sword. It doesn't move. Not yet, anyway.")

            .ReturnTo("Garden", "You step back from the statue.");


        public static Scene FrontGargoyle { get; } =
            NewScene("FrontGargoyle",
                     "Assets/Backgrounds/Mansion/FrontGargoyle.png",
                     "The sad garogyle watches you carefully with its stoneface.")

            .Hotspot("Gargoyle", 780, 200, 400, 440)
                .Describe("Its stone eyes seem to follow you. Perhaps it wants to talk.")
                .TalkStarts("SadGargoyle")

            .ReturnTo("FrontGate", "You leave the sad gargoyle to its sad and boring fate.");


        public static Scene FrontDoor { get; } =
      NewScene(
          "FrontDoor",
          "Assets/Backgrounds/Mansion/Front_door.png",
          "The heavy front door towers over you. "
          + "A grotesque metal knocker hangs at its centre.")

          // =====================================================
          // BACKGROUND STATE
          // =====================================================

          // Once Quazimodo has admitted the player, accepted payment,
          // or been defeated, show the doors open.
          .BackgroundWhen(
              p => p.HasFlag("FrontDoorOpen"),
              "Assets/Backgrounds/Mansion/Front_door_open.png")

          // =====================================================
          // CLOSED FRONT DOORS
          // =====================================================

          .Hotspot(
              "Front Doors",
              590,
              300,
              375,
              500)

              .Describe(
                  "Heavy wooden front doors. "
                  + "They appear to be locked.")

              .OnTalk(
                  "The door does not answer. "
                  + "This is unsurprising.")

              .ActStarts("QuazimodoFrontDoor")

              .OnlyWhen(
                  p => !p.HasFlag("FrontDoorOpen"))

          // =====================================================
          // OPEN FRONT DOORS
          // =====================================================

          .Hotspot(
              "Open Front Doors",
              590,
              300,
              375,
              500)

              .Describe(
                  "The enormous front doors stand open. "
                  + "Beyond them waits the dark entrance hall.")

              .GoesTo("EntranceHall")

              .OnlyWhen(
                  p => p.HasFlag("FrontDoorOpen"))

          // =====================================================
          // DOOR KNOCKER
          // =====================================================

          .Hotspot(
              "Door Knocker",
              685,
              515,
              175,
              90)

              .Describe(
                  "The grotesque iron door knocker is freezing cold.")

              .OnAct(
                  "You rap the iron knocker against the door. "
                  + "The sound echoes somewhere deep inside the Mansion.")

              .OnItem(
                  "Gold Coins",
                  "You rap the knocker with a gold coin. "
                  + "Nothing. Manners, not money, the gargoyle said.")

              .OnItem(
                  "Royal Gold Coin",
                  "You rap the knocker with the King's coin. "
                  + "Nothing. Manners, not money, the gargoyle said.")

              .OnlyWhen(
                  p => !p.HasFlag("FrontDoorOpen"))

          // =====================================================
          // RETURN
          // =====================================================

          .ReturnTo(
              "Courtyard",
              "You step away from the front door.");

        // =========================================================
        // LOOKUP (used by the game - no need to edit)
        // =========================================================

        internal static void Register(Scene scene) => SceneLookup[scene.Name] = scene;

        // Every scene in the game (used by the debug menu).
        public static IEnumerable<Scene> All => SceneLookup.Values;

        public static Scene? GetScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return null;
            }

            SceneLookup.TryGetValue(sceneName, out Scene? scene);
            return scene;
        }
    }
}
