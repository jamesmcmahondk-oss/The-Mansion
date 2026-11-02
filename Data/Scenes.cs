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
                .Describe("An ancient stone fountain. Something glitters beneath the water.")
                .GoesTo("FountainCloseup")

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

            .ReturnTo("FrontGate", "You step away from the fountain.");


        public static Scene FrontGargoyle { get; } =
            NewScene("FrontGargoyle",
                     "Assets/Backgrounds/Mansion/FrontGargoyle.png",
                     "The sad garogyle watches you carefully with its stoneface.")

            .Hotspot("Gargoyle", 780, 200, 400, 440)
                .Describe("Its stone eyes seem to follow you. Perhaps it wants to talk.")
                .TalkStarts("SadGargoyle")

            .ReturnTo("FrontGate", "You leave the sad gargoyle to its sad and boring fate.");


        public static Scene FrontDoor { get; } =
    NewScene("FrontDoor",
             "Assets/Backgrounds/Mansion/Front_door.png",
             "The heavy front door towers over you. A grotesque metal knocker hangs at its centre.")

        // Before Quazimodo has answered the door.
        .Hotspot("Front Doors", 590, 300, 375, 500)
            .Describe("Heavy wooden front doors. They appear to be locked.")
            .OnTalk("The door does not answer. This is unsurprising.")
            .ActStarts("QuazimodoFrontDoor")
            .OnlyWhen(p => !p.HasFlag("FrontDoorOpen"))

        // After Quazimodo has admitted the player.
        .Hotspot("Open Front Doors", 590, 300, 375, 500)
            .Describe("The enormous doors now stand slightly open. Darkness waits beyond them.")
            .GoesTo("EntranceHall")
            .OnlyWhen(p => p.HasFlag("FrontDoorOpen"))

        .Hotspot("Door Knocker", 685, 515, 175, 90)
            .Describe("The grotesque iron door knocker is freezing cold.")
            .OnAct(
                "You rap the iron knocker against the door. The sound echoes somewhere deep inside the Mansion.")
            .OnItem(
                "Gold Coins",
                "You rap the knocker with a gold coin. Nothing. Manners, not money, the gargoyle said.")
            .OnItem(
                "Royal Gold Coin",
                "You rap the knocker with the King's coin. Nothing. Manners, not money, the gargoyle said.")
            .OnlyWhen(p => !p.HasFlag("FrontDoorOpen"))

        .ReturnTo("FrontGate", "You step away from the front door.");


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
