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
                .GoesTo("MansionRight")

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


        // =========================================================
        // RIGHT OF THE MANSION: side path -> old well -> graveyard
        // Backgrounds made by Tools/build_wide_backgrounds.py.
        // =========================================================

        public static Scene MansionRight { get; } =
            NewScene("MansionRight",
                     "Assets/Backgrounds/Mansion/mansion_right.png",
                     "A wet stone path runs along the mansion's right wing, then climbs a misty hill towards an old well.")

            .Hotspot("Lantern", 60, 290, 140, 160)
                .Describe("An iron lantern on a stone post. Its flame burns steadily, despite the rain.")

            .Hotspot("Windows", 240, 192, 450, 320)
                .Describe("Candles burn behind the tall windows, but nothing moves inside.")

            .Hotspot("Hill Path", 960, 450, 320, 340)
                .Describe("The path winds up the hill to an old well.")
                .GoesTo("Well")

            .Hotspot("Old Well", 1170, 290, 140, 160)
                .Describe("A small roofed well stands alone at the top of the hill.")
                .GoesTo("Well")

            .ReturnTo("Courtyard", "You head back to the courtyard.");


        public static Scene Well { get; } =
            NewScene("Well",
                     "Assets/Backgrounds/Mansion/well.png",
                     "An old stone well stands under a mossy roof. Beyond it, a path leads down to a fenced graveyard.")

            .Hotspot("Well", 290, 590, 470, 220)
                .Describe("Cold air rises from the well. You lean over to look inside...")
                .GoesTo("WellCloseup")
                .OnTalk("\"Hello?\" ...Hello? ...hello? The echo takes rather too long to fade.")

            .Hotspot("Bucket", 470, 490, 95, 110)
                .Describe("A rusty bucket hangs from a chain over the well. It's empty.")
                .OnAct("You crank the bucket down into the dark. It comes back up empty - and somehow colder.")

            .Hotspot("Lantern", 810, 375, 70, 145)
                .Describe("A lantern hangs from the well's post, swinging gently in a wind you can't feel.")

            .Hotspot("Graveyard", 1000, 520, 500, 100)
                .Describe("Gravestones crowd the hillside beyond the iron fence.")
                .GoesTo("GraveyardEntrance")

            .Hotspot("Graveyard Path", 1050, 620, 300, 210)
                .Describe("A narrow path leads down past lit lanterns to the graveyard.")
                .GoesTo("GraveyardEntrance")

            .ReturnTo("MansionRight", "You walk back down the hill.");


        public static Scene GraveyardEntrance { get; } =
            NewScene("GraveyardEntrance",
                     "Assets/Backgrounds/Mansion/graveyard_entrance.png",
                     "The graveyard gates stand open. Mossy headstones lean beneath an ancient tree, and a stone cross rises over the hill.")

            .Hotspot("Old Tree", 330, 192, 220, 300)
                .Describe("A huge, gnarled oak. Its roots have tipped several headstones over.")

            .Hotspot("Lantern", 130, 230, 130, 150)
                .Describe("A lantern on the gatepost lights the way in. Someone keeps these burning.")

            .Hotspot("Lantern", 1250, 420, 110, 140)
                .Describe("A lantern on the gatepost lights the way in. Someone keeps these burning.")

            .Hotspot("Old Gravestone", 580, 410, 120, 200)
                .Describe("The inscription has worn away to almost nothing. Almost.")
                .GoesTo("GravestoneCloseup")

            .Hotspot("Stone Cross", 1000, 260, 140, 250)
                .Describe("A tall Celtic cross stands at the top of the steps, watching over the graves.")
                .GoesTo("CrossCloseup")

            .Hotspot("Graveyard Path", 870, 560, 220, 240)
                .Describe("Mossy steps lead up between the graves, deeper into the graveyard.")

            .ReturnTo("Well", "You leave the graveyard behind.");


        // Looking down the well. The key can be fished out with the Ball of
        // String (always works) or the bucket (a Luck test, try again).
        public static Scene WellCloseup { get; } =
            NewScene("WellCloseup",
                     "Assets/Backgrounds/Mansion/well_closeup.png",
                     "You peer down the well. Cobwebs stretch across the slick stones, and far below, something small glints in the dark.")

            // The key is gone from the picture once you have it.
            .BackgroundWhen(p => p.HasFlag("GotWellKey"),
                            "Assets/Backgrounds/Mansion/well_closeup_empty.png")

            .Hotspot("Well Shaft", 380, 280, 820, 420)
                .Describe("The shaft drops away into darkness. The stones are wet and furred with cobwebs.")
                .OnTalk("\"Hello down there?\" ...there. ...there. Something at the bottom seems to listen.")

            .Hotspot("Bucket", 0, 300, 240, 350)
                .Describe("The rusty bucket hangs from its chain at the edge of the well.")
                .OnAct(p =>
                {
                    if (p.HasFlag("GotWellKey"))
                    {
                        return "You lower the bucket into the dark. It comes back up with nothing but cold water.";
                    }

                    if (p.TestLuck())
                    {
                        p.Give("Old Key");
                        p.SetFlag("GotWellKey");
                        return "You lower the bucket and scrape it along the bottom... When it comes back up, a small brass key is rattling inside!";
                    }

                    return "The bucket thuds against the bottom and swings straight past the glint. Maybe try again?";
                })

            .Hotspot("Glinting Key", 755, 685, 95, 75)
                .Describe("At the very bottom of the well, a small brass key catches the light.")
                .OnAct("It's far too deep to reach. You'd need something long - or something to fish with.")
                .OnItem("Ball of String",
                        "You tie a loop in the end of the string and lower it down... after a few patient tries the loop catches the key's handle. You reel up a small brass key!",
                        effect: p =>
                        {
                            p.Give("Old Key");
                            p.SetFlag("GotWellKey");
                        })
                .OnlyWhen(p => !p.HasFlag("GotWellKey"))

            .ReturnTo("Well", "You step back from the edge of the well.");


        // Close-ups in the graveyard (fitted by Tools/build_wide_backgrounds.py).
        public static Scene CrossCloseup { get; } =
            NewScene("CrossCloseup",
                     "Assets/Backgrounds/Mansion/cross_closeup.png",
                     "The great stone cross looms over you, every inch of it carved with endless knots.")

            .Hotspot("Moon", 1040, 192, 110, 110)
                .Describe("The full moon hangs over the graveyard. It looks closer than it should.")

            .Hotspot("Celtic Cross", 600, 192, 430, 420)
                .Describe("Knots weave in and out of each other without beginning or end. At the centre, three spirals chase one another.")
                .OnTalk("You read the knots aloud, as if they were words. They are not. You feel slightly foolish.")

            .Hotspot("Plinth", 530, 612, 520, 200)
                .Describe("A mossy stepped plinth. A small knotwork panel is set into its face, as if something might be hidden behind it.")
                .OnAct("You press the carved panel. It doesn't budge - but the stone around it sounds hollow.")
                .OnItem("Old Key", "You search the plinth for a keyhole. There isn't one - not here, anyway.")

            .ReturnTo("GraveyardEntrance", "You step back from the cross.");


        public static Scene GravestoneCloseup { get; } =
            NewScene("GravestoneCloseup",
                     "Assets/Backgrounds/Mansion/gravestone_closeup.png",
                     "An old headstone, smothered in ivy and lichen. Its inscription has almost worn away.")

            .Hotspot("Moon", 1200, 192, 130, 110)
                .Describe("The full moon hangs low over the graveyard.")

            .Hotspot("Gravestone", 525, 200, 505, 630)
                .Describe("Ivy has crept up both sides of the stone, as if trying to hold it upright.")

            .Hotspot("Carved Cross", 740, 250, 150, 220)
                .Describe("A small Celtic cross is carved at the top, the same knotwork as the great cross on the hill.")

            .Hotspot("Inscription", 680, 480, 280, 240)
                .Describe("Most of the letters are gone. Only a few marks remain.")
                .OnAct(p => p.TestWits()
                    ? "You trace the worn letters with your finger: \"...HERE LIES ... FAITHFUL ... KEEPER OF THE KEYS ...\""
                    : "You squint at the worn letters, but they won't come together. Perhaps another look?")

            .ReturnTo("GraveyardEntrance", "You leave the old grave in peace.");


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
