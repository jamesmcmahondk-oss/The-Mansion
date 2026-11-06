using System;
using System.Collections.Generic;
using The_Mansion.Models;

namespace The_Mansion.Data
{
    /*
     * All scenes in the game. See SCENE_GUIDE.md for how to write them.
     *
     * Verbs: LOOK describes a hotspot and opens its .CloseUp(...) scene;
     * ACT does its .OnAct(...) or moves on to its .GoesTo(...) scene;
     * TALK and ITEM do their own things. Anything else gets a
     * "You can't do that." style reply.
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

            // Once Quazimodo has let you in, the doors stand open in the
            // distance too (frontgate_open.png: the same picture with the
            // doorway patched in by Tools/patch_variant.py).
            .BackgroundWhen(p => p.HasFlag("FrontDoorOpen"),
                            "Assets/Backgrounds/Mansion/frontgate_open.png")

            .Hotspot("Fountain", 550, 400, 335, 345)
                .Describe("An ancient stone fountain stands in the courtyard beyond the gates.")
                .GoesTo("Courtyard")

            .Hotspot("Driveway", 470, 760, 640, 72)
                .Describe("Wet cobbles lead through the gates towards the mansion.")
                .GoesTo("Courtyard")

            .Hotspot("Left Gate", 325, 380, 140, 420)
                .Describe("A tall wrought-iron gate, standing open.")
                .OnAct("The gate is already open. It seems to prefer it that way.")

            .Hotspot("Right Gate", 1115, 420, 110, 412)
                .Describe("A tall wrought-iron gate, standing open.")
                .OnAct("The gate is already open. It seems to prefer it that way.")

            .Hotspot("Lantern", 160, 430, 70, 130)
                .Describe("A lantern burns on the gate pillar. Someone keeps these lit.")
                .OnAct("The glass is hot. Whoever lit it wants it to stay lit.")

            .Hotspot("Lantern", 1335, 450, 70, 130)
                .Describe("A lantern burns on the gate pillar. Someone keeps these lit.")
                .OnAct("The glass is hot. Whoever lit it wants it to stay lit.")

            .Hotspot("Coat of Arms", 1330, 610, 85, 115)
                .Describe("A weathered coat of arms is set into the pillar. The carving has worn too smooth to read.")

            // The right-hand gargoyle has left.
            .Hotspot("Empty Pillar", 1235, 240, 220, 200)
                .Describe("The top of the right-hand gate pillar is empty. The moss up there has a gargoyle-shaped gap in it.")
                .OnTalk(p =>
                {
                    if (!p.HasFlag("TalkedToEmptyPillar"))
                    {
                        p.SetFlag("TalkedToEmptyPillar");
                        return "You address the empty pillar. The empty pillar has nothing to add.";
                    }

                    return "You look at the empty pillar. It continues to make a surprisingly persuasive argument.";
                })
                .OnAct("It's well out of reach.")

            .Hotspot("Front Door", 870, 470, 135, 185)
                .Describe("The enormous wooden door looks like it has not opened in years.")
                .GoesTo("FrontDoor")
                .OnlyWhen(p => !p.HasFlag("FrontDoorOpen"))

            .Hotspot("Front Door", 870, 470, 135, 185)
                .Describe("The front doors stand open. Candlelight spills down the steps.")
                .GoesTo("FrontDoor")
                .OnlyWhen(p => p.HasFlag("FrontDoorOpen"))

            .Hotspot("Gargoyle", 115, 195, 205, 160)
                .Describe("A sulky and rather sad looking gargoyle sits here.")
                .CloseUp("FrontGargoyle");


        public static Scene FountainCloseup { get; } =
            NewScene("FountainCloseup",
                     "Assets/Backgrounds/Mansion/fountain_closeup.png",
                     "Lily pads float on the dark water. Spouts of water splash down from the fountain above.")

            // The coins vanish from the picture while you have them, and come
            // back if you return them (Tools/patch_variant.py made the pair).
            .BackgroundWhen(p => p.HasFlag("StoleCoins"),
                            "Assets/Backgrounds/Mansion/fountain_closeup_nocoins.png")

            .Hotspot("Gold Coins", 595, 655, 295, 175)
                .Describe("Several old gold coins rest at the bottom of the fountain.")

                // ACT on the coins: King Hautain appears on the lily pad.
                .ActStarts("HautainCoins")
                .OnlyWhen(p => !p.HasFlag("StoleCoins"))

            .Hotspot("Fountain Floor", 595, 655, 295, 175)
                .Describe("A cleaner patch on the fountain floor, where the coins used to lie.")
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
                .OnlyWhen(p => p.HasFlag("StoleCoins"))

            .Hotspot("Fountain", 1040, 192, 496, 330)
                .Describe("The fountain's carved stone base, green with moss. Above it, stone lions spit water into the basin.")
                .OnTalk("The lions keep their mouths full.")

            .Hotspot("Water Lily", 160, 470, 160, 110)
                .Describe("A single pale water lily, open in the rain.")
                .OnAct("You leave it be. It's the only thing here that looks content.")

            .ReturnTo("Courtyard", "You step away from the fountain.");


        // Between the front gate and the fountain close-up / front door.
        // Background: ChatGPT picture (Nov 2026); the open-door version is patched from a second one.
        public static Scene Courtyard { get; } =
            NewScene("Courtyard",
                     "Assets/Backgrounds/Mansion/courtyard.png",
                     "Rain-slick cobblestones glitter in the lantern light. A tiered fountain splashes beside the steps up to the front door.")

            // Once Quazimodo has let you in, the doors stand open here too
            // (courtyard_open.png: the doorway from a second picture, lined up
            // on the door frame and patched in by Tools/patch_variant.py).
            .BackgroundWhen(p => p.HasFlag("FrontDoorOpen"),
                            "Assets/Backgrounds/Mansion/courtyard_open.png")

            .Hotspot("Fountain", 0, 520, 360, 362)
                .OnTalk("The fountain burbles. It is not a conversation.")
                .Describe("Water spills over the fountain's tiers into a wide basin. Something glitters beneath the surface.")
                .CloseUp("FountainCloseup")

            .Hotspot("Statue", 110, 285, 140, 235)
                .Describe("A robed stone woman stands at the top of the fountain, hands folded, looking away from the house.")
                .OnTalk("She keeps looking away. You get the feeling it's deliberate.")
                .OnAct("She's well out of reach, at the top of the fountain.")

            .Hotspot("Front Door", 595, 250, 550, 470)
                .Describe("Wide stone steps lead up to the mansion's great carved doors.")
                .GoesTo("FrontDoor")
                .OnlyWhen(p => !p.HasFlag("FrontDoorOpen"))

            .Hotspot("Front Door", 595, 250, 550, 470)
                .Describe("At the top of the steps the great doors stand open. Candlelight spills out into the rain.")
                .GoesTo("FrontDoor")
                .OnlyWhen(p => p.HasFlag("FrontDoorOpen"))

            // Listed after the front door so they win where they overlap.
            .Hotspot("Lantern", 675, 240, 55, 130)
                .Describe("An iron lantern beside the door. Someone keeps these lit.")
                .OnAct("Too high to reach from down here.")

            .Hotspot("Lantern", 1010, 240, 55, 130)
                .Describe("An iron lantern beside the door. Someone keeps these lit.")
                .OnAct("Too high to reach from down here.")

            .Hotspot("Urn", 595, 395, 115, 120)
                .Describe("A stone urn on the balustrade, overflowing with ivy and red flowers.")

            .Hotspot("Urn", 1025, 395, 115, 120)
                .Describe("A stone urn on the balustrade, overflowing with ivy and red flowers.")

            .Hotspot("Windows", 280, 192, 330, 250)
                .Describe("Rows of tall Gothic windows, every one of them lit. Nobody appears in any of them.")
                .OnAct("The windows are well out of reach.")

            .Hotspot("Windows", 1130, 192, 260, 250)
                .Describe("Rows of tall Gothic windows, every one of them lit. Nobody appears in any of them.")
                .OnAct("The windows are well out of reach.")

            .Hotspot("Lamp Post", 1440, 360, 96, 160)
                .Describe("A lamp post at the edge of the courtyard, lighting the way round the side of the house.")
                .OnAct("The glass is hot. Whoever lit it wants it to stay lit.")

            .Hotspot("Garden Path", 300, 450, 170, 130)
                .Describe("Past the fountain, a cobbled path winds away towards a hooded statue and a glowing glasshouse.")
                .GoesTo("Garden")

            .Hotspot("Side Path", 1300, 560, 140, 260)
                .Describe("A wet cobbled path curves away round the right side of the mansion.")
                .GoesTo("MansionRight")

            .ReturnTo("FrontGate", "You walk back to the gates.");


        // Left of the courtyard: the overgrown garden with the glasshouse.
        // Background made by Tools/build_wide_backgrounds.py.
        public static Scene Garden { get; } =
            NewScene("Garden",
                     "Assets/Backgrounds/Mansion/garden.png",
                     "The path ends in an overgrown garden. Mist curls around a hooded statue, and beyond it a great glasshouse glows in the dark.")

            // The statue's hands are empty once you've taken its sword
            // (backgrounds built by Tools/build_garden.py).
            .BackgroundWhen(p => p.HasFlag("TookStoneSword"),
                            "Assets/Backgrounds/Mansion/garden_nosword.png")

            .Hotspot("Glasshouse", 430, 330, 600, 180)
                .OnAct("It's a long way to walk just to push on glass. A closer look first.")
                .Describe("A vast glasshouse of iron and glass. Candlelight flickers inside, though nobody seems to be tending it.")
                .CloseUp("GlasshouseCloseup")

            .Hotspot("Mansion", 1090, 192, 446, 450)
                .Describe("Ivy smothers the mansion's left wing. Warm light glows in the tall windows.")
                .OnAct("The windows are well out of reach.")

            .Hotspot("Lantern", 40, 290, 150, 170)
                .Describe("A lantern on a stone pillar lights the garden path. Someone keeps these lit.")
                .OnAct("The glass is hot. Whoever lit it wants it to stay lit.")

            .Hotspot("Hydrangeas", 1260, 560, 276, 200)
                .Describe("Pink hydrangeas, heavy with rain, crowd the foot of the mansion wall.")

            .Hotspot("Garden Path", 280, 640, 520, 192)
                .Describe("The wet path winds back towards the courtyard.")
                .GoesTo("Courtyard")

            // Listed after the glasshouse so it wins where they overlap.
            .Hotspot("Statue", 775, 340, 120, 300)
                .OnTalk("The statue is too far away to hear you. Probably.")
                .Describe("A hooded stone figure stands on a plinth in the long grass, watching the path.")
                .CloseUp("StatueCloseup")

            .ReturnTo("Courtyard", "You follow the path back to the courtyard.");


        public static Scene StatueCloseup { get; } =
            NewScene("StatueCloseup",
                     "Assets/Backgrounds/Mansion/statue_closeup.png",
                     "The statue towers over you: a hooded figure in a ragged stone robe, its hands resting on the hilt of a great sword.")

            // Hands folded around nothing once you've taken the sword.
            .BackgroundWhen(p => p.HasFlag("TookStoneSword"),
                            "Assets/Backgrounds/Mansion/statue_closeup_nosword.png")

            .Hotspot("Glasshouse", 294, 291, 336, 386)
                .Describe("Through the mist, candles glow behind the glasshouse's tall panes.")

            .Hotspot("Glasshouse", 901, 291, 131, 386)
                .Describe("Through the mist, candles glow behind the glasshouse's tall panes.")

            .Hotspot("Mansion", 1098, 192, 300, 509)
                .Describe("The mansion's ivy-covered wall rises beside the statue, every window lit.")
                .OnAct("The windows are well out of reach.")

            .Hotspot("Statue", 630, 209, 271, 542)
                .Describe("Ivy creeps up the ragged stone robe. Under the hood there is only darkness.")
                .OnTalk("The statue says nothing. Statues usually don't. Usually.")

            // Listed after the statue so they win where they overlap.
            .Hotspot("Hood", 704, 209, 115, 115)
                .OnAct("You reach up into the hood. Stone, all the way back.")
                .Describe("You peer into the hood. There is no face carved there at all - just shadow.")

            // THE FACELESS KNIGHT: the sword only comes free for someone who
            // isn't holding a weapon of their own (put it away by clicking
            // WEAPON in the stats panel).
            .Hotspot("Stone Sword", 747, 332, 54, 418)
                .Describe("A great stone sword, its point resting on the plinth. The hilt is worn smooth, as if many hands have gripped it. " +
                          "Words are cut along the blade: HE WHO WOULD TAKE THE BLADE MUST FIRST SURRENDER HIS OWN.")
                .LookSetsFlag("ReadStoneSword")
                .OnAct(p =>
                {
                    bool armed = !string.IsNullOrWhiteSpace(p.Weapon) && p.Weapon != "None";

                    if (armed)
                    {
                        return "You grip the hilt. The stone fingers tighten. Nothing moves.";
                    }

                    p.Give("Stone Sword");
                    p.Weapon = "Stone Sword";
                    p.SetFlag("TookStoneSword");
                    return "You take hold of the hilt with empty hands. GRIND. The stone fingers slowly open... and the sword comes free. " +
                           "It is far lighter than it looks.";
                })
                .OnlyWhen(p => !p.HasFlag("TookStoneSword"))

            .Hotspot("Empty Hands", 712, 348, 107, 107)
                .Describe("The statue's stone hands are folded around nothing now. Under the hood, the darkness seems... satisfied.")
                .OnlyWhen(p => p.HasFlag("TookStoneSword"))

            .ReturnTo("Garden", "You step back from the statue.");


        // The glasshouse on the left of the mansion (from the Garden).
        public static Scene GlasshouseCloseup { get; } =
            NewScene("GlasshouseCloseup",
                     "Assets/Backgrounds/Mansion/glasshouse_closeup.png",
                     "The glasshouse rises before you, all iron lace and misted glass. Inside, hundreds of candles burn among tall palms.")

            .Hotspot("Glass Dome", 470, 192, 630, 60)
                .Describe("Rain beads on the great glass dome. Ivy has crept almost to the top.")

            .Hotspot("Windows", 60, 410, 330, 310)
                .OnAct("The windows are well out of reach.")
                .Describe("Through the misted panes you can make out palms, ferns - and candles, far too many candles.")

            .Hotspot("Windows", 1210, 410, 320, 310)
                .OnAct("The windows are well out of reach.")
                .Describe("Through the misted panes you can make out palms, ferns - and candles, far too many candles.")

            .Hotspot("Lantern", 612, 545, 58, 130)
                .OnAct("The glass is hot. Whoever lit it wants it to stay lit.")
                .Describe("An iron lantern on a stone pillar, lighting the steps.")

            .Hotspot("Lantern", 960, 545, 58, 130)
                .OnAct("The glass is hot. Whoever lit it wants it to stay lit.")
                .Describe("An iron lantern on a stone pillar, lighting the steps.")

            .Hotspot("Chandelier", 775, 445, 70, 60)
                .OnAct("It's on the other side of the glass. Which is probably for the best.")
                .Describe("A chandelier blazes just inside, above the doors. Someone lit every candle.")

            .Hotspot("Glasshouse Doors", 725, 500, 170, 275)
                .OnTalk("You call through the glass. Nobody answers. The candles burn on regardless.")
                .OnOtherItems("You hold the {item} up to the glass. Nothing inside takes any notice.")
                .Describe("Tall glass doors with brass handles. The air seeping out is warm and smells of earth and wax.")
                .OnAct("You push the doors. They don't open - yet they rattle, as if something inside pushed back.",
                       p => p.SetFlag("TriedGlasshouseDoors"))

            .ReturnTo("Garden", "You walk back to the garden.");


        // =========================================================
        // RIGHT OF THE MANSION: side path -> old well -> graveyard
        // Backgrounds made by Tools/build_wide_backgrounds.py.
        // =========================================================

        public static Scene MansionRight { get; } =
            NewScene("MansionRight",
                     "Assets/Backgrounds/Mansion/mansion_right.png",
                     "A wet stone path runs along the Mansion's right wing, between candlelit windows and a wall of roses. " +
                     "Ahead, an archway opens onto steps climbing to an old well.")

            // One rose is gone from the picture once you've cut it.
            .BackgroundWhen(p => p.HasFlag("GotRedRose"),
                            "Assets/Backgrounds/Mansion/mansion_right_rose_cut.png")

            .Hotspot("Windows", 140, 260, 130, 270)
                .OnAct("The windows are well out of reach.")
                .Describe("Candles burn behind the tall windows, but nothing moves inside.")

            .Hotspot("Windows", 440, 200, 100, 330)
                .OnAct("The windows are well out of reach.")
                .Describe("Candles burn behind the tall windows, but nothing moves inside.")

            .Hotspot("Windows", 585, 280, 75, 230)
                .OnAct("The windows are well out of reach.")
                .Describe("Candles burn behind the tall windows, but nothing moves inside.")

            .Hotspot("Balustrade", 0, 560, 700, 272)
                .Describe("A heavy stone balustrade runs along the foot of the wall, slick with rain.")

            .Hotspot("Old Well", 870, 230, 120, 150)
                .OnTalk("You'd need to be a lot closer to talk down a well.")
                .Describe("A small roofed well stands at the top of the hill.")
                .GoesTo("Well")

            .Hotspot("Garden Arch", 800, 350, 145, 330)
                .Describe("An ivy-covered archway in the garden wall. Steps beyond it climb towards the well.")
                .GoesTo("Well")

            // The path slips past the lantern, around the corner of the house.
            .Hotspot("Path to the Back", 700, 420, 100, 160)
                .Describe("Beside the lantern, the cobbles slip round the corner of the Mansion, towards the back of the house.")
                .GoesTo("RearYard")

            .Hotspot("Lantern", 760, 435, 45, 70)
                .OnAct("The glass is hot. Whoever lit it wants it to stay lit.")
                .Describe("A lantern burns on the garden wall. Its flame burns steadily, despite the rain.")

            .Hotspot("Rose Bush", 980, 460, 480, 230)
                .OnTalk("You compliment the roses. The thorns remain unpersuaded.")
                .OnOtherItems("The rose bush is unimpressed by the {item}.")
                .Describe("A great tangle of blood-red roses, impossibly in bloom this late in the year.")
                .LookSetsFlag("SawRoseBush")
                .OnlyWhen(p => !p.HasFlag("GotRedRose"))
                .OnAct(p => Roses.Touch(p))
                .OnItem("Garden Shears", "Snip. Much better. You cut one perfect dark-red rose.",
                        effect: p => Roses.Cut(p),
                        onlyIf: p => !p.HasFlag("GotRedRose"))

            .Hotspot("Rose Bush", 980, 460, 480, 230)
                .OnTalk("You compliment the roses. The thorns remain unpersuaded.")
                .OnOtherItems("The rose bush is unimpressed by the {item}.")
                .Describe("A great tangle of blood-red roses. There's a small gap where you cut one.")
                .OnlyWhen(p => p.HasFlag("GotRedRose"))
                .OnAct(p => Roses.Touch(p))
                .OnItem("Garden Shears", "You already have a rose. Leave the rest of them be.")

            .Hotspot("Hydrangeas", 1340, 690, 196, 142)
                .OnAct("You leave them be. They look like they've had a difficult night.")
                .Describe("Pale hydrangeas, heavy with rain. They have lost all their colour.")

            // The bench: sit on it (ACT) first. Then you hear something
            // underneath, and LOOK opens the close-up with Ebria.
            .Hotspot("Stone Bench", 1085, 700, 250, 132)
                .OnTalk("The bench says nothing. Benches rarely do.")
                .Describe("A mossy stone bench beside the roses.")
                .OnAct("You sit down on the cold, damp stone... and from somewhere underneath you comes a faint hiccup. Then a tiny, slurred giggle.",
                       p => p.SetFlag("HeardBenchNoise"))
                .OnlyWhen(p => !p.HasFlag("HeardBenchNoise"))

            .Hotspot("Stone Bench", 1085, 700, 250, 132)
                .Describe("Something is hiccuping under the bench.")
                .CloseUp("BenchCloseup")
                .OnAct("You sit down again. The hiccuping stops. Then: \"...I can hear you breathing.\"")
                .OnlyWhen(p => p.HasFlag("HeardBenchNoise"))

            .ReturnTo("Courtyard", "You head back to the courtyard.");


        // =========================================================
        // REAR YARD - the service side of the Mansion (from MansionRight).
        // No characters: exploration, resources (shears, charcoal, rope),
        // and future shortcuts (kitchen door, cellar). See PUZZLES.md.
        // =========================================================

        public static Scene RearYard { get; } =
            NewScene("RearYard",
                     "Assets/Backgrounds/Mansion/rear_yard.png",
                     "A rain-soaked service yard behind the Mansion. Ordinary work seems to have simply stopped here.")

            .Hotspot("Path Back", 0, 450, 95, 330)
                .Describe("The wet path continues around the side of the Mansion.")
                .GoesTo("MansionRight")

            .Hotspot("Path Around Mansion", 1440, 420, 96, 360)
                .Describe("The narrow service path continues around the Mansion. Something glass-roofed is visible through the trees.")
                .GoesTo("KitchenGarden")

            .Hotspot("Ivy", 200, 192, 300, 330)
                .Describe("Ivy has claimed most of the rear wall. The Mansion rises almost vertically above the little service yard.")

            .Hotspot("Windows", 160, 245, 70, 175)
                .OnAct("The windows are well out of reach.")
                .Describe("Narrow service windows overlook the yard. A few glow warmly despite the abandoned courtyard below.")

            .Hotspot("Windows", 410, 283, 45, 96)
                .OnAct("The windows are well out of reach.")
                .Describe("Narrow service windows overlook the yard. A few glow warmly despite the abandoned courtyard below.")

            .Hotspot("Lantern", 560, 208, 55, 107)
                .OnAct("The glass is hot. Whoever lit it wants it to stay lit.")
                .Describe("A lantern burns beside the service door. Someone keeps these lit.")

            // Bolted from inside - never opened by a key. Later: drawing the
            // bolt from the kitchen sets KitchenDoorUnlocked and this becomes
            // a shortcut (.GoesTo("Kitchen") with .OnlyWhen).
            .Hotspot("Kitchen Door", 603, 245, 192, 363)
                .OnTalk("You call out. The bolt on the other side doesn't care who you are.")
                .OnOtherItems("There's nothing on this side of the door for the {item} to work on.")
                .Describe("A heavy service door leads into the Mansion, with a small barred window and heavy iron hardware. " +
                          "Generations of boots have worn the edges of the stone steps smooth.")
                .OnAct("The door shifts half an inch, then stops against something solid. Bolted from the other side.",
                       p => p.SetFlag("TriedKitchenDoor"))

            // Something underneath holds them shut. The knock happens once.
            .Hotspot("Cellar Doors", 256, 544, 300, 128)
                .OnTalk("You lean close to the gap and say hello. The rain answers instead.")
                .OnOtherItems("The {item} won't shift whatever is holding the doors.")
                .Describe("A pair of heavy cellar doors descend beneath the Mansion. The left door doesn't quite sit flush with its frame.")
                .OnAct("You pull the iron ring. The door rises an inch. Stops. Something underneath is holding it shut. ... " +
                       "A single quiet sound comes from underneath. Knock.",
                       p => p.SetFlag("TriedCellarDoor"))
                .OnlyWhen(p => !p.HasFlag("TriedCellarDoor"))

            .Hotspot("Cellar Doors", 256, 544, 300, 128)
                .OnTalk("You lean close to the gap and say hello. The rain answers instead.")
                .OnOtherItems("The {item} won't shift whatever is holding the doors.")
                .Describe("A pair of heavy cellar doors descend beneath the Mansion. The left door doesn't quite sit flush with its frame.")
                .OnAct("Still blocked. Whatever is beneath it isn't moving.")
                .OnlyWhen(p => p.HasFlag("TriedCellarDoor"))

            .Hotspot("Clothesline", 810, 288, 240, 22)
                .Describe("An old washing line stretches across the yard. Almost everything that once hung from it is gone.")

            // It is simply The Sock.
            .Hotspot("Sock", 933, 322, 30, 48)
                .OnTalk("The Sock has nothing to say.")
                .OnOtherItems("You and the {item} leave the Sock in peace.")
                .Describe("A solitary black sock hangs from the washing line.")
                .OnAct(p =>
                {
                    if (p.HasFlag("TouchedSock"))
                    {
                        return "Still damp.";
                    }

                    p.SetFlag("TouchedSock");
                    return "Damp.";
                })

            .Hotspot("Hand Pump", 875, 390, 165, 120)
                .Describe("An old iron hand pump feeds the stone washing trough. The metal is rusted, but the mechanism appears intact.")
                .OnAct(p =>
                {
                    if (p.HasFlag("TroughFilled"))
                    {
                        return "Cold water splashes into the already-full trough.";
                    }

                    p.SetFlag("TroughFilled");
                    return "The handle complains loudly. After several strokes, cold water spills into the trough.";
                })

            .Hotspot("Stone Trough", 800, 510, 245, 58)
                .Describe("A deep stone trough stained by generations of water.")
                .OnAct("Dry, apart from rain collecting in the bottom.")
                .OnlyWhen(p => !p.HasFlag("TroughFilled"))

            .Hotspot("Stone Trough", 800, 510, 245, 58)
                .Describe("Cold rainwater and pump water fill the old stone trough.")
                .OnAct("The water is painfully cold.")
                .OnlyWhen(p => p.HasFlag("TroughFilled"))

            .Hotspot("Drain", 760, 728, 75, 38)
                .OnTalk("Your voice drops down the drain and doesn't come back up.")
                .Describe("Rainwater disappears through an iron grate beneath the Mansion. " +
                          "A few tiny pale fragments have collected against the grate. Probably kitchen scraps.")
                .OnAct("The grate is firmly set into the cobbles.")

            // ---- The tool shed --------------------------------------
            // The door leads into the close-up (ToolShedCloseup), where the
            // shears and charcoal lie on the workbench.
            .Hotspot("Tool Shed", 990, 192, 480, 205)
                .OnAct("The door is the part that opens.")
                .Describe("A small gardening shed leans against the wall, its roof thick with moss. " +
                          "The tools outside have been left exactly where someone last used them. Apparently nobody came back for them.")

            .Hotspot("Shed Door", 1061, 400, 118, 293)
                .Sound("door_creak")
                .Describe("A plank door with an iron ring for a handle. It isn't locked.")
                .CloseUp("ToolShedCloseup")
                .GoesTo("ToolShedCloseup")

            .Hotspot("Rope", 1179, 411, 26, 82)
                .Describe("A coil of old rope hangs on the shed wall. Damp, but stronger than it looks.")
                .OnAct("You lift the coil of rope off its hook.",
                       p =>
                       {
                           p.Give("Rope");
                           p.SetFlag("GotRope");
                       })
                .OnlyWhen(p => !p.HasFlag("GotRope"))

            .Hotspot("Shovel", 1205, 411, 43, 229)
                .Describe("A heavy garden shovel hangs from a hook rusted solid around its handle.")
                .OnAct("It isn't coming free without taking half the shed with it.")

            .Hotspot("Pitchfork", 1248, 411, 44, 229)
                .OnAct("You'd rather not carry a pitchfork around. It invites questions.")
                .Describe("An old pitchfork. Several tines have rusted away.")

            .Hotspot("Rake", 1292, 411, 75, 229)
                .OnAct("You leave the rake where it lies. Stepping on it later is a tradition you can do without.")
                .Describe("A rake with half its teeth missing.")

            .ReturnTo("MansionRight", "You head back along the side of the Mansion.");


        // Inside the tool shed (from the Rear Yard). The shears and the
        // charcoal can be taken in any order; the picture changes to match
        // (backgrounds built by Tools/build_tool_shed.py). The cabinet is
        // locked and stays locked - the shed is not a lock puzzle.
        public static Scene ToolShedCloseup { get; } =
            NewScene("ToolShedCloseup",
                     "Assets/Backgrounds/Mansion/tool_shed_closeup.png",
                     "The shed door scrapes open. Inside it smells of rust, damp wood and old soil. A lantern burns on the roof beam.")

            .BackgroundWhen(p => p.HasFlag("GotGardenShears") && p.HasFlag("GotCharcoal"),
                            "Assets/Backgrounds/Mansion/tool_shed_closeup_empty.png")
            .BackgroundWhen(p => p.HasFlag("GotGardenShears"),
                            "Assets/Backgrounds/Mansion/tool_shed_closeup_no_shears.png")
            .BackgroundWhen(p => p.HasFlag("GotCharcoal"),
                            "Assets/Backgrounds/Mansion/tool_shed_closeup_no_charcoal.png")

            .Hotspot("Shelves", 310, 192, 695, 215)
                .Describe("Rusted tins with no labels, and terracotta pots - stacked, tipped over, one of them broken.")
                .OnAct("You prise the lid off a tin. Rust, and the smell of old nails. You put the lid back.")

            .Hotspot("Lantern", 985, 192, 110, 90)
                .Describe("A small oil lantern hangs from the roof beam, lit. Someone keeps these lit.")
                .OnAct("The glass is hot. Whoever lit it wants it to stay lit.")

            .Hotspot("Shed Door", 60, 300, 205, 532)
                .Describe("The door hangs open behind you. Rain drips from its iron ring.")
                .GoesTo("RearYard")

            .Hotspot("Cabinet", 1185, 192, 315, 640)
                .Describe("A tall, narrow cabinet, closed with a heavy iron padlock. Through the gaps in its boards you can see more rusted tins.")
                .OnAct("Locked, and not worth the trouble. It's only more tins.")
                .OnOtherItems("The {item} won't open the padlock. Not that the tins inside are worth it.")

            .Hotspot("Workbench", 265, 520, 915, 130)
                .Describe("A scarred old workbench, worn smooth where hands rested on it for years.")
                .OnAct("You run a hand along the bench. Splinters, mostly.")

            .Hotspot("Garden Shears", 485, 495, 265, 95)
                .Describe("A sturdy pair of pruning shears - rusty, but still serviceable.")
                .OnAct("You take the garden shears.",
                       p =>
                       {
                           p.Give("Garden Shears");
                           p.SetFlag("GotGardenShears");
                       })
                .OnlyWhen(p => !p.HasFlag("GotGardenShears"))

            .Hotspot("Charcoal", 870, 500, 175, 85)
                .Describe("A few sticks of charcoal. Messy, but still usable.")
                .OnAct("You take the charcoal, and most of its dust with it.",
                       p =>
                       {
                           p.Give("Charcoal");
                           p.SetFlag("GotCharcoal");
                       })
                .OnlyWhen(p => !p.HasFlag("GotCharcoal"))

            .Hotspot("Watering Can", 380, 700, 285, 132)
                .Describe("An old galvanised watering can, dented and freckled with rust.")
                .OnAct("Empty. It's raining anyway.")

            .Hotspot("Seed Crate", 665, 740, 240, 92)
                .Describe("A wooden seed crate.")
                .OnAct("Empty, apart from a few dead leaves.")

            .Hotspot("Flower Pots", 910, 715, 180, 117)
                .Describe("More terracotta pots, stacked under the bench.")

            .ReturnTo("RearYard", "You step back out into the rain.");


        // =========================================================
        // KITCHEN GARDEN - walled vegetable garden behind the Mansion
        // (from the Rear Yard). The door in the wall leads through to the
        // Garden and glasshouse, closing the loop around the Mansion.
        // =========================================================

        public static Scene KitchenGarden { get; } =
            NewScene("KitchenGarden",
                     "Assets/Backgrounds/Mansion/kitchen_garden.png",
                     "A walled kitchen garden. The beds are still planted and weeded, as if the gardener only just stepped out. Beyond a door in the wall, the glasshouse glows.")

            .Hotspot("Garden Wall", 267, 267, 1269, 180)
                .Describe("A high brick wall, thick with ivy. It keeps the wind out - and anything else.")

            .Hotspot("Windows", 30, 290, 80, 200)
                .OnAct("The windows are well out of reach.")
                .Describe("Candles burn in the Mansion's windows. Someone keeps a lot of candles.")

            .Hotspot("Windows", 160, 192, 65, 270)
                .OnAct("The windows are well out of reach.")
                .Describe("Candles burn in the Mansion's windows. Someone keeps a lot of candles.")

            .Hotspot("Rain Barrels", 229, 448, 166, 150)
                .Describe("Two old rain barrels, full to the brim.")
                .OnAct("You peer into the barrel. Your reflection peers back. It takes a moment longer than it should.")

            .Hotspot("Potting Bench", 400, 448, 171, 140)
                .Describe("A potting bench with a few terracotta pots and a seed box. Everything has been put away neatly.")

            // Not the tool shed's shears - these are scenery.
            .Hotspot("Hedge Clippers", 475, 462, 88, 36)
                .Describe("A pair of hedge clippers, rusted completely shut.")
                .OnAct("They won't open. Whoever left them here hasn't been back for a very long time.")

            .Hotspot("Bean Poles", 608, 437, 197, 208)
                .Describe("Two wigwams of bean poles. Nothing grows on them now except a few dead tendrils.")

            .Hotspot("Garden Door", 800, 363, 181, 213)
                .Describe("An open door in the wall. Through it, the glasshouse glows in the mist.")
                .GoesTo("Garden")

            .Hotspot("Vegetable Beds", 997, 470, 539, 170)
                .Describe("Raised beds of cabbages and leeks, fat and healthy despite the season. Someone still tends them.")
                .OnAct("You pull at a leek. It doesn't budge. It feels... rooted rather more firmly than leeks ought to be.")

            .Hotspot("Cabbage", 693, 651, 160, 106)
                .Describe("An enormous purple cabbage. It is, objectively, a magnificent cabbage.")
                .OnTalk("You compliment the cabbage. It accepts graciously.")

            .Hotspot("Plant Label", 933, 640, 54, 107)
                .Describe("A wooden plant label. Whatever was written on it has washed away.")

            .Hotspot("Flower Pots", 341, 699, 171, 122)
                .Describe("A few terracotta pots, one of them cracked and tipped on its side.")

            .Hotspot("Path Back", 0, 597, 213, 192)
                .Describe("The path leads back to the service yard.")
                .GoesTo("RearYard")

            .ReturnTo("RearYard", "You head back to the service yard.");


        // Close-up of the stone bench and the rose bush (from MansionRight).
        // Ebria the drunk fairy sits underneath the bench.
        public static Scene BenchCloseup { get; } =
            NewScene("BenchCloseup",
                     "Assets/Backgrounds/Mansion/bench_closeup.png",
                     "A carved stone bench stands against the wall, half swallowed by an enormous rose bush. Something tiny hiccups underneath it.")

            // Background and sprite split from the painted-over picture by
            // Tools/split_bench_fairy.py.
            .Sprite("Assets/Characters/Ebria_UnderBench.png", 415, 396, 422)

            .Hotspot("Ferns", 0, 300, 140, 230)
                .Describe("Ferns and lilies crowd the path, dripping with rain.")

            .Hotspot("Stone Bench", 140, 200, 1396, 630)
                .Describe("The bench is carved with quatrefoils and scrolls. Rain has pooled in the seat.")
                .OnAct("You sit down. From underneath comes an indignant \"Oi! Some of us live here!\"")

            .Hotspot("Rose Bush", 830, 630, 240, 200)
                .Describe("The rose bush has grown right under the bench. Its thorns glint in the dark.")
                .OnAct(p => Roses.Touch(p))
                .OnItem("Garden Shears", "Snip. Much better. You cut one perfect dark-red rose.",
                        effect: p => Roses.Cut(p),
                        onlyIf: p => !p.HasFlag("GotRedRose"))
                .OnItem("Garden Shears", "You already have a rose. Leave the rest of them be.")

            // Listed last so she wins over the bench.
            .Hotspot("Ebria", 440, 425, 280, 390)
                .Describe("A tiny fairy with wild red hair sits under the bench, swigging from a bottle labelled ANOTHER BAD IDEA.")
                .TalkStarts("Ebria")

            .ReturnTo("MansionRight", "You leave the bench and its tenant in peace.");


        public static Scene Well { get; } =
            NewScene("Well",
                     "Assets/Backgrounds/Mansion/well.png",
                     "An old stone well stands under a mossy roof. Beyond it, a path leads down to a fenced graveyard.")

            .Hotspot("Well", 290, 590, 470, 220)
                .OnOtherItems("You consider dropping the {item} down the well. You decide you might want it back.")
                .Describe("Cold air rises from the well. You lean over to look inside...")
                .CloseUp("WellCloseup")
                .OnTalk("\"Hello?\" ...Hello? ...hello? The echo takes rather too long to fade.")

            .Hotspot("Bucket", 470, 490, 95, 110)
                .Describe("A rusty bucket hangs from a chain over the well. It's empty.")
                .OnAct("You crank the bucket down into the dark. It comes back up empty - and somehow colder.")

            .Hotspot("Lantern", 810, 375, 70, 145)
                .OnAct("The glass is hot. Whoever lit it wants it to stay lit.")
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

            .Hotspot("Old Tree", 300, 192, 450, 348)
                .OnAct("You'd rather not climb it in the rain. Or at all.")
                .Describe("A huge, gnarled oak. Its roots have tipped several headstones over.")

            .Hotspot("Lantern", 115, 300, 160, 200)
                .OnAct("The glass is hot. Whoever lit it wants it to stay lit.")
                .Describe("A lantern on the gatepost lights the way in. Someone keeps these burning.")

            .Hotspot("Lantern", 1265, 445, 130, 170)
                .OnAct("The glass is hot. Whoever lit it wants it to stay lit.")
                .Describe("A lantern on the gatepost lights the way in. Someone keeps these burning.")

            .Hotspot("Old Gravestone", 580, 525, 145, 230)
                .Describe("The inscription has worn away to almost nothing. Almost.")
                .CloseUp("GravestoneCloseup")

            .Hotspot("Stone Cross", 1000, 290, 170, 300)
                .Describe("A tall Celtic cross stands at the top of the steps, watching over the graves.")
                .CloseUp("CrossCloseup")

            .Hotspot("Graveyard Path", 1040, 620, 140, 140)
                .Describe("Mossy steps lead up between the graves, deeper into the graveyard.")

            .ReturnTo("Well", "You leave the graveyard behind.");


        // Looking down the well. Something silver can be fished out with the
        // Ball of String (always works) or the bucket (a Luck test, try again):
        // the Cracked Hand Mirror.
        public static Scene WellCloseup { get; } =
            NewScene("WellCloseup",
                     "Assets/Backgrounds/Mansion/well_closeup.png",
                     "You peer down the well. Cobwebs stretch across the slick stones, and far below, something small glints in the dark.")

            // The glint is gone from the picture once you have it.
            .BackgroundWhen(p => p.HasFlag("GotWellMirror"),
                            "Assets/Backgrounds/Mansion/well_closeup_empty.png")

            .Hotspot("Well Shaft", 380, 215, 820, 420)
                .Describe("The shaft drops away into darkness. The stones are wet and furred with cobwebs.")
                .OnTalk("\"Hello down there?\" ...there. ...there. Something at the bottom seems to listen.")

            .Hotspot("Bucket", 0, 300, 240, 290)
                .Describe("The rusty bucket hangs from its chain at the edge of the well.")
                .OnAct(p =>
                {
                    if (p.HasFlag("GotWellMirror"))
                    {
                        return "You lower the bucket into the dark. It comes back up with nothing but cold water.";
                    }

                    if (p.Meets("Luck", WellLuck))
                    {
                        p.Give("Cracked Hand Mirror");
                        p.SetFlag("GotWellMirror");
                        return $"[Luck {WellLuck}] You lower the bucket and scrape it along the bottom... When it comes back up, a small silver hand mirror is lying in it. The glass is cracked straight across.";
                    }

                    return $"[Luck {WellLuck} needed - yours is {p.Luck}] The bucket thuds against the bottom and swings straight past the glint, every time. " +
                           "Luck isn't going to do this for you. Something to fish with might.";
                })
                .NeedsStat("Luck", WellLuck)

            .Hotspot("Something Glinting", 772, 702, 66, 54)
                .Describe("At the very bottom of the well, something small and silver catches the light.")
                .LookSetsFlag("SawWellGlint")
                .OnAct("It's far too deep to reach. You'd need something long - or something to fish with.")
                .OnItem("Ball of String",
                        "You tie a loop in the end of the string and lower it down... after a few patient tries the loop catches a handle. You reel up a small silver hand mirror. The glass is cracked straight across.",
                        effect: p =>
                        {
                            p.Give("Cracked Hand Mirror");
                            p.SetFlag("GotWellMirror");
                        })
                .OnlyWhen(p => !p.HasFlag("GotWellMirror"))

            .ReturnTo("Well", "You step back from the edge of the well.");


        // Close-ups in the graveyard (fitted by Tools/build_wide_backgrounds.py).
        public static Scene CrossCloseup { get; } =
            NewScene("CrossCloseup",
                     "Assets/Backgrounds/Mansion/cross_closeup.png",
                     "The great stone cross looms over you, every inch of it carved with endless knots.")

            // The knotwork panel slides aside when the spirals are solved.
            .BackgroundWhen(p => p.HasFlag("CrossOpened"),
                            "Assets/Backgrounds/Mansion/cross_closeup_open.png")

            .Hotspot("Moon", 1078, 192, 117, 106)
                .OnAct("Out of reach. Comfortably.")
                .OnTalk("The moon has heard it all before.")
                .Describe("The full moon hangs over the graveyard. It looks closer than it should.")

            .Hotspot("Celtic Cross", 614, 196, 320, 430)
                .OnOtherItems("You hold the {item} up to the cross. The spirals don't care.")
                .Describe("Knots weave in and out of each other without beginning or end. At the centre, three spirals chase one another: " +
                          "one reaching up, one to the left, one to the right.")
                .OnTalk("You read the knots aloud, as if they were words. They are not. You feel slightly foolish.")
                .LookSetsFlag("SawCrossSpirals")
                .ActOpensPuzzle("Cross")

            // THE GRAVEYARD SECRET: ACT on the spirals (or the cross) opens the
            // turning-spiral puzzle (Data/CrossPuzzle.cs, MainWindow.CrossPuzzle.cs).
            .Hotspot("Spirals", 740, 315, 90, 75)
                .Describe("At the very centre of the cross, three stone spirals are set into the stone, each in its own round socket.")
                .LookSetsFlag("SawCrossSpirals")
                .ActOpensPuzzle("Cross")

            .Hotspot("Plinth", 544, 626, 468, 206)
                .Describe("A mossy stepped plinth. A small knotwork panel is set into its face, as if something might be hidden behind it.")
                .OnAct("You press the carved panel. It doesn't budge - but the stone around it sounds hollow.")
                .OnlyWhen(p => !p.HasFlag("CrossOpened"))

            .Hotspot("Open Plinth", 544, 626, 468, 206)
                .Describe("The knotwork panel in the plinth has slid aside. Something lies in the hollow behind it.")
                .OnAct("Inside the hollow lies a tiny, old photograph. As your fingers close around it, the rain stops. " +
                       "Not slows. Stops. You remember sunlight. You don't remember remembering sunlight. Then the rain resumes. " +
                       "(Memento: Worn Photograph)",
                       p =>
                       {
                           p.Give("Worn Photograph");
                           p.SetFlag("FoundPhotograph");
                       })
                .ShowsPicture("Assets/Items/Sources/worn_photograph.png")
                .OnlyWhen(p => p.HasFlag("CrossOpened") && !p.HasFlag("FoundPhotograph"))

            .Hotspot("Empty Plinth", 544, 626, 468, 206)
                .Describe("The hollow in the plinth is empty now.")
                .OnlyWhen(p => p.HasFlag("FoundPhotograph"))

            .ReturnTo("GraveyardEntrance", "You step back from the cross.");


        public static Scene GravestoneCloseup { get; } =
            NewScene("GravestoneCloseup",
                     "Assets/Backgrounds/Mansion/gravestone_closeup.png",
                     "An old headstone, smothered in ivy and lichen. Its inscription has almost worn away.")

            .Hotspot("Moon", 1205, 192, 140, 62)
                .OnAct("Out of reach. Comfortably.")
                .OnTalk("The moon has heard it all before.")
                .Describe("The full moon hangs low over the graveyard.")

            .Hotspot("Gravestone", 490, 192, 560, 640)
                .OnTalk("You speak to the stone. Whoever lies here stopped listening long ago.")
                .Describe("Ivy has crept up both sides of the stone, as if trying to hold it upright.")

            .Hotspot("Carved Cross", 725, 197, 165, 240)
                .Describe("A small Celtic cross is carved at the top, the same knotwork as the great cross on the hill.")

            // Stats create shortcuts; items provide reliable solutions:
            // ACT is a Wits test (retry allowed), Charcoal + Paper always works.
            .Hotspot("Inscription", 600, 457, 360, 280)
                .OnOtherItems("The {item} won't help you read the stone.")
                .Describe("Most of the inscription has been worn smooth. A few shallow marks remain, but not enough to read.")
                .OnAct(p => Gravestone.Read(p))
                .NeedsStat("Wits", Gravestone.WitsNeeded)
                .OnItem("Charcoal", Gravestone.KnownText, onlyIf: p => p.HasFlag("ReadGravestone"))
                .OnItem("Charcoal", Gravestone.RubbingText,
                        effect: p => p.SetFlag("ReadGravestone"),
                        onlyIf: p => p.Has("Paper"))
                .OnItem("Charcoal", "Charcoal straight onto the stone would only add marks. You need something to take a rubbing on.")
                .OnItem("Paper", Gravestone.KnownText, onlyIf: p => p.HasFlag("ReadGravestone"))
                .OnItem("Paper", Gravestone.RubbingText,
                        effect: p => p.SetFlag("ReadGravestone"),
                        onlyIf: p => p.Has("Charcoal"))
                .OnItem("Paper", "You press the paper against the stone. Without something to rub across it, it stays blank.")

            .ReturnTo("GraveyardEntrance", "You leave the old grave in peace.");


        public static Scene FrontGargoyle { get; } =
            NewScene("FrontGargoyle",
                     "Assets/Backgrounds/Mansion/FrontGargoyle.png",
                     "The sad gargoyle watches you carefully with its stone face.")

            .Hotspot("Gargoyle", 420, 192, 760, 640)
                .Describe("Its stone eyes seem to follow you. Perhaps it wants to talk.")
                .TalkStarts("SadGargoyle")

            .ReturnTo("FrontGate", "You leave the sad gargoyle to its sad and boring fate.");


        public static Scene FrontDoor { get; } =
      NewScene(
          "FrontDoor",
          "Assets/Backgrounds/Mansion/Front_door.png",    // fitted above the HUD (source: Sources/Front_door_full.png)
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
          // THE DOORS (picture pair: the open doorway was patched into the
          // closed picture by Tools/patch_variant.py, so nothing else moves)
          // =====================================================

          .Hotspot("Front Doors", 618, 286, 312, 386)
              .Describe("Heavy carved wooden doors under a pointed arch, with a window of stone tracery above. They appear to be locked.")
              .OnTalk("The door does not answer. This is unsurprising.")
              .ActStarts("QuazimodoFrontDoor")
              .OnlyWhen(p => !p.HasFlag("FrontDoorOpen"))

          .Hotspot("Open Front Doors", 618, 286, 312, 386)
              .Sound("door_creak")
              .Describe("The front doors stand open. Beyond them, candlelight, a chequered floor and a grand staircase.")
              .GoesTo("EntranceHall")
              .OnlyWhen(p => p.HasFlag("FrontDoorOpen"))

          // Listed after the doors so they win where they overlap.
          .Hotspot("Door Knocker", 686, 434, 62, 82)
              .Sound("door_knock")
              .Describe("A grotesque iron knocker: a snarling face with a heavy ring through its jaws. It is freezing cold.")
              .OnAct("You rap the iron knocker against the door. The sound echoes somewhere deep inside the Mansion.")
              .OnItem("Gold Coins", "You rap the knocker with a gold coin. Nothing. Manners, not money, the gargoyle said.")
              .OnItem("Royal Gold Coin", "You rap the knocker with the King's coin. Nothing. Manners, not money, the gargoyle said.")
              .OnlyWhen(p => !p.HasFlag("FrontDoorOpen"))

          .Hotspot("Door Knocker", 794, 434, 62, 82)
              .Sound("door_knock")
              .Describe("A grotesque iron knocker: a snarling face with a heavy ring through its jaws. It is freezing cold.")
              .OnAct("You rap the iron knocker against the door. The sound echoes somewhere deep inside the Mansion.")
              .OnItem("Gold Coins", "You rap the knocker with a gold coin. Nothing. Manners, not money, the gargoyle said.")
              .OnItem("Royal Gold Coin", "You rap the knocker with the King's coin. Nothing. Manners, not money, the gargoyle said.")
              .OnlyWhen(p => !p.HasFlag("FrontDoorOpen"))

          .Hotspot("Window", 548, 192, 418, 71)
              .Describe("Stone tracery above the doors, glowing with candlelight from inside.")
              .OnAct("Far out of reach.")

          .Hotspot("Lantern", 425, 192, 90, 223)
              .Describe("An iron lantern beside the door. Someone keeps these lit.")
              .OnAct("The glass is hot. Whoever lit it wants it to stay lit.")

          .Hotspot("Lantern", 1032, 192, 90, 223)
              .Describe("An iron lantern beside the door. Someone keeps these lit.")
              .OnAct("The glass is hot. Whoever lit it wants it to stay lit.")

          .Hotspot("Urn", 298, 421, 164, 213)
              .Describe("A stone urn overflowing with ivy and small red flowers, wet with rain.")
              .OnAct("You'd rather not dig through someone's flowerpot.")

          .Hotspot("Urn", 1082, 421, 164, 213)
              .Describe("A stone urn overflowing with ivy and small red flowers, wet with rain.")
              .OnAct("You'd rather not dig through someone's flowerpot.")

          // =====================================================
          // RETURN
          // =====================================================

          .ReturnTo(
              "Courtyard",
              "You step away from the front door.");


        // =========================================================
        // INSIDE THE MANSION
        // =========================================================

        // The Entrance Hall - the hub of the house (see ENTRANCE_HALL.md).
        // The rooms beyond aren't built yet: the doors and the stairs give a
        // holding reply (marked "not built yet") until they are.
        public static Scene EntranceHall { get; } =
            NewScene("EntranceHall",
                     "Assets/Backgrounds/Mansion/entrance_hall.png",
                     "The great hall of the Mansion. Every candle is lit, and the house is very quiet. The rain sounds far away now.")

            .Hotspot("Staircase", 440, 360, 420, 450)
                .Describe("A grand staircase, carpeted in faded red, climbs to a landing and turns up into the dark.")
                .OnAct("You put a hand on the banister and look up into the dark. Not yet.")    // not built yet: upstairs

            .Hotspot("Landing", 760, 290, 390, 220)
                .Describe("The stairs turn at a landing and climb on along the right-hand wall, into shadow.")
                .OnAct("You put a hand on the banister and look up into the dark. Not yet.")    // not built yet: upstairs

            .Hotspot("Window", 560, 192, 195, 280)
                .Describe("A tall window above the stairs, framed by heavy red curtains. Rain streams down the glass.")
                .OnAct("The window is far above the stairs, and it doesn't look as if it opens.")

            .Hotspot("Chandelier", 640, 192, 300, 80)
                .Describe("A great brass chandelier, every candle lit. Fine silver threads are strung between its arms, glinting.")
                .OnAct("Far out of reach.")

            .Hotspot("Family Portrait", 275, 192, 170, 385)
                .Describe("A family portrait in a heavy gilt frame: five people in their best clothes. " +
                          "Where one face should be, the paint has flaked away to bare canvas.")
                .OnAct("The frame is fixed firmly to the wall.")
                .OnTalk("Nobody in the portrait answers. One of them couldn't if they wanted to.")
                .OnItem("Worn Photograph",
                        "You hold the photograph up beside the portrait. Your eyes keep going to the same place in both: the face that isn't there.")

            // To the Portrait Gallery - not locked.
            .Hotspot("Left Door", 135, 300, 100, 520)
                .Describe("A tall panelled door on the left side of the hall.")
                .GoesTo("PortraitGallery")
                .Sound("door_creak")

            .Hotspot("Right Door", 1385, 240, 140, 592)
                .Describe("A tall panelled door on the right side of the hall.")
                .OnAct("The handle turns, but the door doesn't open. Not yet.")    // not built yet: dining room

            .Hotspot("Low Door", 895, 545, 105, 190)
                .Describe("A small, low door, tucked in under the stairs.")
                .OnAct("It doesn't open.")    // not built yet: cellar (no key - see PUZZLES.md)

            .Hotspot("Grandfather Clock", 1075, 385, 85, 447)
                .Describe("A tall grandfather clock. Its hands have stopped at a quarter past something.")
                .OnAct("You nudge the pendulum. It swings once, then stops again.")
                .OnTalk("The clock has nothing to say. It has stopped saying anything at all.")

            // Never explained (like the reflection in the rain barrel).
            .Hotspot("Covered Mirror", 1160, 385, 180, 447)
                .Describe("A tall mirror, covered completely by a grey dust sheet.")
                .OnAct("You take hold of a corner of the sheet... and decide to leave it where it is.")

            .Hotspot("Candles", 1265, 280, 70, 90)
                .Describe("Candles burn in a brass sconce. Someone keeps these lit.")
                .OnAct("The brass is hot. Whoever lit them wants them to stay lit.")

            .Hotspot("Candles", 1035, 230, 55, 65)
                .Describe("Candles burn in a brass sconce on the landing. Someone keeps these lit.")
                .OnAct("Out of reach from down here.")

            .Hotspot("Coat Stand", 10, 380, 160, 452)
                .Describe("A crooked coat stand. An old coat hangs from it, still damp.")
                .OnAct("You leave the coat where it is. It isn't yours.")
                .OnTalk("\"Good evening, Quazimodo.\" It is a coat stand.")

            // Listed last so she wins over the stairs.
            .Hotspot("Cat", 540, 610, 65, 100)
                .Describe("A black cat sits on the stairs, watching you with unblinking yellow eyes.")
                .OnTalk("You say hello. The cat considers this, and looks away.")
                .OnAct("You reach towards the cat. It shifts exactly far enough to stay out of reach.")
                .OnOtherItems("The cat looks at the {item}, then at you, with deep disappointment.")

            .ReturnTo("FrontDoor", "You step back out into the rain.");

        // =========================================================
        // FIRST FLOOR
        // =========================================================

        // The Portrait Gallery, through the left door of the Entrance Hall.
        // A person can change without ceasing to be the same person.
        // THE MISSING PORTRAIT: the empty frame is a door. Its three brass
        // medallions match the three age portraits and are pressed in the
        // order the portraits were painted (Gallery helper below). Behind it:
        // the Portrait Ledger (read it with right-click: Dialogues.PortraitLedger).
        public static Scene PortraitGallery { get; } =
            NewScene("PortraitGallery",
                     "Assets/Backgrounds/Mansion/portrait_gallery.png",
                     "")
            .DescribeScene(p =>
                "The Portrait Gallery stretches along one side of the Mansion. " +
                "Cold moonlight enters through tall Gothic windows, while candles and the fireplace fill the opposite wall with warm gold. " +
                "Portraits crowd the red wallpaper in frames of every size. Some have names. Some have lost them. " +
                (p.HasFlag("SolvedPortraitGalleryPuzzle")
                    ? "One ornate frame now stands open, exposing a narrow compartment inside the wall."
                    : "One very large frame contains nothing at all."))

            // Stays open for good once solved.
            .BackgroundWhen(p => p.HasFlag("SolvedPortraitGalleryPuzzle"),
                            "Assets/Backgrounds/Mansion/portrait_gallery_open.png")

            // Listed first: the specific paintings below win where they overlap.
            .Hotspot("Other Portraits", 590, 192, 520, 360)
                .Describe(p =>
                {
                    if (p.HasFlag("LookedAtOtherPortraits"))
                    {
                        return "Different people. Different years. The same carefully arranged importance.";
                    }
                    p.SetFlag("LookedAtOtherPortraits");
                    return "Generations of unfamiliar faces look back from expensive frames. " +
                           "Some still have names beneath them. Others have only empty brackets.";
                })
                .OnAct("You inspect several frames. None appears eager to become an inventory item.")
                .OnTalk("Nobody answers. The portraits are only portraits.")

            .Hotspot("Other Portraits", 1350, 585, 90, 140)
                .Describe(p => p.HasFlag("LookedAtOtherPortraits")
                    ? "Different people. Different years. The same carefully arranged importance."
                    : "Generations of unfamiliar faces look back from expensive frames. " +
                      "Some still have names beneath them. Others have only empty brackets.")
                .LookSetsFlag("LookedAtOtherPortraits")
                .OnAct("You inspect several frames. None appears eager to become an inventory item.")

            // 1. THE FAMILY PORTRAIT - on the Mansion steps, around a space no one occupies.
            .Hotspot("Family Portrait", 1115, 192, 405, 395)
                .Describe(p =>
                    "A formal family portrait painted on the Mansion steps. You've stood exactly where they are standing. " +
                    "Judging by the expressions, sitting for the portrait was considered compulsory. " +
                    "Something about the arrangement feels unbalanced. Several figures appear positioned around a space that no one occupies.")
                .LookSetsFlag("ExaminedFamilyPortrait")
                .OnAct("The frame is fixed firmly to the wall. Whatever bothers you is in the painting, not behind it.")
                .OnTalk("Nobody in the portrait answers. Nobody in the portrait ever looks as if they might.")
                .OnItem("Worn Photograph",
                        "The photograph and portrait disagree about many details. They agree about the absence.",
                        onlyIf: p => p.HasFlag("ComparedPhotographToPortrait"))
                .OnItem("Worn Photograph",
                        "You hold the photograph beside the painting. Different clothes. Different arrangement. Perhaps a different year. " +
                        "Your eyes keep returning to the same place in both. The place where someone should be.",
                        effect: p => p.SetFlag("ComparedPhotographToPortrait"))
                .OnOtherItems("You hold the {item} up to the portrait. Nobody in it is interested.")

            // 2. THREE AGES - Mirélle, painted decades apart (hung out of order:
            // 1821, 1798, 1854). Part of the empty-frame puzzle.
            .Hotspot("Three Ages", 790, 315, 200, 195)
                .Describe("Three portraits hang in matching frames.")
                .LookSetsFlag("ExaminedThreeAges")
                .CloseUp("ThreeAgesCloseup")
                .OnAct("The three frames are fixed to the wall. Small engraved dates remain beneath them: " +
                       "1821 under the left, 1798 under the centre, 1854 under the right.",
                       p => p.SetFlag("ExaminedThreeAges"))
                .OnTalk("The same woman looks back three times. None of her answers.")

            // The old painting of the gates - both gargoyles still on their pillars.
            .Hotspot("Gate Painting", 685, 385, 78, 195)
                .Describe(p =>
                {
                    string text = "An old painting of the Mansion gates. Two stone gargoyles watch from the pillars.";
                    if (p.HasFlag("MetDunan"))
                    {
                        return text + " Dúnán looks considerably less patient in this painting.";
                    }
                    if (p.HasFlag("MetGargoyle"))
                    {
                        return text + " So the missing gargoyle wasn't a joke. Well. Not entirely.";
                    }
                    return text;
                })
                .OnAct("The painting is fixed to the wall. Both gargoyles stay where they are. In the painting, at least.")

            // Lady Vesper - never labelled.
            .Hotspot("Red Portrait", 608, 280, 80, 295)
                .Describe("An exquisitely dressed figure regards the room from a carved gilt frame.")
                .CloseUp("VesperPortraitCloseup")
                .OnAct(p => p.HasFlag("MetVesper")
                    ? "You keep your own hands well away from the frame."
                    : "You're not certain why that bothers you.")

            // Pip - just Pip.
            .Hotspot("Pencil Sketch", 788, 558, 45, 80)
                .Describe(p =>
                {
                    if (!p.HasFlag("MetPip"))
                    {
                        return "A quick pencil sketch of someone holding a stuffed bear, behind cheap glass. " +
                               "Compared with everything else in the room, it seems strangely ordinary.";
                    }
                    return p.HasFlag("AskedPipAboutBear")
                        ? "That's Pip. The bear hasn't changed much either. Apparently Bear was very reasonably priced."
                        : "That's Pip. The bear hasn't changed much either.";
                })
                .OnAct("The sketch hangs on a single nail. You leave it exactly where somebody put it.")

            // 10. STUDIES - eyes, hands, flowers, fragments of buildings.
            .Hotspot("Studies", 836, 543, 78, 85)
                .Describe(p => Gallery.Studies(p))
                .OnAct("The smaller frames are fixed securely to the wall.")
            .Hotspot("Studies", 928, 548, 74, 125)
                .Describe(p => Gallery.Studies(p))
                .OnAct("The smaller frames are fixed securely to the wall.")
            .Hotspot("Studies", 1013, 593, 70, 70)
                .Describe(p => Gallery.Studies(p))
                .OnAct("The smaller frames are fixed securely to the wall.")
            .Hotspot("Studies", 1188, 583, 160, 150)
                .Describe(p => Gallery.Studies(p))
                .OnAct("The smaller frames are fixed securely to the wall.")
            .Hotspot("Studies", 1438, 598, 95, 150)
                .Describe(p => Gallery.Studies(p))
                .OnAct("The smaller frames are fixed securely to the wall.")

            // 9. ARCHITECTURAL SKETCH - a clue towards the empty frame.
            .Hotspot("Architectural Sketch", 1092, 588, 75, 105)
                .Describe("An old architectural study of the Mansion. Several windows do not appear to match the building outside. " +
                          "You may be remembering it incorrectly.")
                .LookSetsFlag("ExaminedGallerySketch")
                .OnAct("The study is fixed securely to the wall.")

            // 13. GOTHIC WINDOWS
            .Hotspot("Gothic Windows", 15, 192, 75, 425)
                .Describe("Tall Gothic windows overlook the dark grounds. Rain traces silver paths down the glass.")
                .OnAct("The windows are latched. Cold air still finds a way through.")
            .Hotspot("Gothic Windows", 180, 240, 40, 360)
                .Describe("Tall Gothic windows overlook the dark grounds. Rain traces silver paths down the glass.")
                .OnAct("The windows are latched. Cold air still finds a way through.")

            // 7. THE TALL MIRROR - a slow instrument, never a jumpscare.
            .Hotspot("Tall Mirror", 85, 225, 115, 530)
                .Describe(p => Gallery.MirrorLook(p))
                .OnAct(p => Gallery.MementoCount(p) >= 1
                    ? "You adjust your posture. The reflection does the same. That ought to be more reassuring than it is."
                    : "You adjust your posture. The reflection does the same. Reassuring.")
                .OnTalk("You say hello to your reflection. It says hello back, at exactly the same time. Fair enough.")
                .OnItem("Cracked Hand Mirror", "You hold the little mirror up to the big one. A crack runs through one of the reflections. Only one.")
                .OnOtherItems("The reflection holds up a {item} as well. Neither of you knows why.")

            // 8. FIREPLACE
            .Hotspot("Fireplace", 320, 300, 165, 310)
                .Describe("The fireplace burns brightly beneath the Mansion crest. Someone has tended it recently.")
                .OnAct(p => Gallery.MementoCount(p) >= 1
                    ? "You hold your hands toward the fire. For a moment, you remember doing this somewhere else. " +
                      "The memory disappears before the room appears."
                    : "Warm. Actually warm.")
                .OnOtherItems("You'd rather not feed the {item} to the fire.")

            // 15. FAR DOOR - the rest of the first floor isn't built yet.
            .Hotspot("Far Door", 228, 405, 90, 195)
                .Describe("A dark wooden door stands at the far end of the gallery.")
                .OnAct("The door opens onto a corridor that has not yet been explored.")    // not built yet: first floor corridor

            // 3. THE EMPTY FRAME - really a shallow door.
            .Hotspot("Empty Frame", 480, 350, 100, 200)
                .Describe("An elaborate frame surrounding nothing but wallpaper. " +
                          "Judging by the cleaner rectangle inside it, something once covered this space.")
                .LookSetsFlag("ExaminedEmptyFrame")
                .OnAct(p => Gallery.TouchFrame(p))
                .NeedsStat("Wits", Gallery.WitsNeeded)
                .OnOtherItems("You try the {item} against the frame. The frame is unmoved.")
                .OnlyWhen(p => !p.HasFlag("SolvedPortraitGalleryPuzzle"))

            // 4. THE CONTROLS - a close-up of the three medallions (FrameCloseup).
            // Listed after the frame so it wins.
            .Hotspot("Medallions", 495, 508, 72, 40)
                .Describe("Three small brass medallions are set into the lower edge of the frame.")
                .CloseUp("FrameCloseup")
                .GoesTo("FrameCloseup")
                .OnlyWhen(p => p.HasFlag("DiscoveredPortraitFrameMechanism") && !p.HasFlag("SolvedPortraitGalleryPuzzle"))

            // 5. THE HIDDEN COMPARTMENT, once solved (the frame swung open).
            .Hotspot("Open Frame", 466, 330, 50, 240)
                .Describe("The ornate frame has swung open on hidden hinges, like a door.")
                .OnAct("It stays open. Whatever it was guarding, it no longer seems to mind.")
                .OnlyWhen(p => p.HasFlag("SolvedPortraitGalleryPuzzle"))
            .Hotspot("Hidden Compartment", 515, 362, 67, 195)
                .Describe(p => p.HasFlag("GotPortraitLedger")
                    ? "A shallow compartment has been built into the wall behind the frame. The hidden recess is empty."
                    : "A shallow compartment has been built into the wall behind the frame. It contains a single leather-bound ledger.")
                .OnAct(p => p.HasFlag("GotPortraitLedger")
                    ? "The hidden recess is empty."
                    : "The compartment is too shallow to conceal anything else.")
                .OnlyWhen(p => p.HasFlag("SolvedPortraitGalleryPuzzle"))

            // 6. THE PORTRAIT LEDGER - listed after the compartment so it wins.
            .Hotspot("Portrait Ledger", 522, 455, 50, 42)
                .Describe("A heavy leather-bound volume rests inside the compartment. Its cover bears the Mansion's crest and the words: PORTRAIT COMMISSIONS")
                .OnAct("You lift the ledger from the compartment. Dust has settled everywhere except along the edge of the cover. " +
                       "(Right-click it in your inventory to read it.)",
                       p =>
                       {
                           p.Give("Portrait Ledger");
                           p.SetFlag("GotPortraitLedger");
                       })
                .OnlyWhen(p => p.HasFlag("SolvedPortraitGalleryPuzzle") && !p.HasFlag("GotPortraitLedger"))

            // 14. Back down to the Entrance Hall (the doorway is behind you).
            .ReturnTo("EntranceHall", "A tall doorway leads back toward the Entrance Hall.");

        // The three portraits of Mirélle, up close (LOOK on Three Ages).
        // Hung out of order: 1821, 1798, 1854 - the puzzle's clue.
        public static Scene ThreeAgesCloseup { get; } =
            NewScene("ThreeAgesCloseup",
                     "Assets/Backgrounds/Mansion/three_ages_closeup.png",
                     "")
            .DescribeScene(p =>
            {
                Gallery.CheckMirelleUnderstanding(p);
                return p.HasFlag("MetMirelle")
                    ? "You know that face. All three of them. Mirélle was painted at three different ages. The expression changed less than the face."
                    : "Three portraits hang in matching frames. The same eyes. The same posture. Different years. The brass nameplates have been removed.";
            })

            .Hotspot("Left Portrait", 299, 214, 291, 438)
                .Describe("The woman in middle age. The first grey in the gold, faint lines at the eyes - and the same guarded look as the others.")
                .OnAct("The frame is fixed to the wall.")
                .OnTalk(p => p.HasFlag("MetMirelle") ? "You decide not to tell this one she looks well. Not out loud." : "She doesn't answer.")

            .Hotspot("Centre Portrait", 616, 214, 293, 438)
                .Describe("The same woman, young - the youngest of the three. Golden hair, no grey at all, the chin lifted.")
                .OnAct("The frame is fixed to the wall.")
                .OnTalk("She doesn't answer. She looks as if she never would have.")

            .Hotspot("Right Portrait", 935, 214, 293, 438)
                .Describe("The same woman, old. Silver hair, deep lines, and exactly the same proud chin. Nothing about her has been diminished. Only changed.")
                .OnAct("The frame is fixed to the wall.")
                .OnTalk("She doesn't answer. Her expression suggests she has heard better questions.")

            .Hotspot("Left Date", 380, 678, 129, 33)
                .Describe("A small brass plate under the left portrait: 1821. Empty screw holes either side, where a nameplate used to be.")
                .OnAct("The plate is screwed firmly to the wall.")
            .Hotspot("Centre Date", 700, 678, 127, 33)
                .Describe("A small brass plate under the centre portrait: 1798. Empty screw holes either side, where a nameplate used to be.")
                .OnAct("The plate is screwed firmly to the wall.")
            .Hotspot("Right Date", 1018, 678, 126, 33)
                .Describe("A small brass plate under the right portrait: 1854. Empty screw holes either side, where a nameplate used to be.")
                .OnAct("The plate is screwed firmly to the wall.")

            .Hotspot("Candles", 1247, 243, 89, 280)
                .Describe("Candles burn in a brass sconce. Someone keeps these lit.")
                .OnAct("The brass is hot.")

            .ReturnTo("PortraitGallery", "You step back from the three portraits.");

        // Lady Vesper's portrait, up close (LOOK on the Red Portrait). Never labelled.
        public static Scene VesperPortraitCloseup { get; } =
            NewScene("VesperPortraitCloseup",
                     "Assets/Backgrounds/Mansion/vesper_portrait_closeup.png",
                     "")
            .DescribeScene(p => p.HasFlag("MetVesper")
                ? "An exquisitely dressed figure regards the room from a carved gilt frame. The painter devoted an unusual amount of attention to the hands. Oh. That's why."
                : "An exquisitely dressed figure regards the room from a carved gilt frame. The painter devoted an unusual amount of attention to the hands.")

            .Hotspot("Portrait", 568, 199, 399, 538)
                .Describe("A seated woman in red and black, rubies at her throat, looking down at you with complete composure. She seems to have already decided what you're worth.")
                .OnAct(p => p.HasFlag("MetVesper")
                    ? "You keep your own hands well away from the frame."
                    : "You're not certain why that bothers you.")
                .OnTalk("You say good evening to the portrait. It feels like the polite thing to do. It also feels like a mistake.")

            // Listed after the portrait so they win.
            .Hotspot("Face", 694, 292, 115, 104)
                .Describe(p => p.HasFlag("MetVesper")
                    ? "Her lips are closed. You now know exactly why the painter preferred them that way."
                    : "Dark red lips, closed. Something about the line of her mouth suggests a smile would be a mistake. For someone.")
                .OnAct("You'd rather not touch her face. Even the painted one.")

            .Hotspot("Hands", 659, 548, 150, 121)
                .Describe("Her hands rest in her lap: long, pale, perfect fingers and a ruby ring. They are the most carefully painted thing in the room. Possibly in the house.")
                .OnAct(p => p.HasFlag("MetVesper")
                    ? "You put your own hands in your pockets."
                    : "You're not certain why that bothers you.")

            .Hotspot("Candles", 399, 292, 121, 249)
                .Describe("Candles burn in a brass sconce beside the portrait. The light makes her skin look paler, not warmer.")
                .OnAct("The brass is hot.")

            .ReturnTo("PortraitGallery", "You step back from the portrait. Her eyes do not follow you. You check.");

        // THE MISSING PORTRAIT, up close: the three brass medallions on the
        // empty frame, each engraved with the same woman's profile at a
        // different age (left: middle age, centre: young, right: old - as the
        // portraits hang). Pressed youngest to oldest (Gallery.Press); a
        // pressed medallion stays down (the sprites). Solving it returns to
        // the gallery with the frame open.
        public static Scene FrameCloseup { get; } =
            NewScene("FrameCloseup",
                     "Assets/Backgrounds/Mansion/frame_closeup.png",
                     "The lower edge of the empty frame. Three brass medallions are set into the gilt, each engraved with a woman's profile.")

            .Sprite("Assets/Backgrounds/Mansion/frame_medallion_centre_pressed.png", 644, 452, 252,
                    p => p.HasFlag("PortraitSequenceStep1") || p.HasFlag("PortraitSequenceStep2"))
            .Sprite("Assets/Backgrounds/Mansion/frame_medallion_left_pressed.png", 251, 457, 252,
                    p => p.HasFlag("PortraitSequenceStep2"))

            .Hotspot("Wallpaper", 200, 192, 1150, 260)
                .Describe("Inside the frame there is only wallpaper - brighter than the rest of the wall, as if something hung here for a very long time.")
                .OnAct("You press the wallpaper. It is a wall.")

            .Hotspot("Candles", 1440, 192, 96, 160)
                .Describe("Candles burn in a brass sconce beside the frame. Someone keeps these lit.")
                .OnAct("The brass is hot.")

            .Hotspot("Left Medallion", 251, 457, 252, 252)
                .Describe(p => "A woman's profile under a wide-brimmed hat crowned with roses and a little skull: no longer young, faint lines at the eyes, the mouth set firm." +
                               (p.HasFlag("MetMirelle") ? " Mirélle, somewhere in the middle of her life." : ""))
                .OnAct(p => Gallery.Press(p, "Left"))
                .Sound("puzzle_click")
                .ThenGoesTo(p => p.HasFlag("SolvedPortraitGalleryPuzzle") ? "PortraitGallery" : "")

            .Hotspot("Centre Medallion", 644, 452, 252, 252)
                .Describe(p => "The same woman under the same hat, young. Long wavy hair loose over her shoulders, the chin lifted." +
                               (p.HasFlag("MetMirelle") ? " Mirélle, before anyone had painted her." : ""))
                .OnAct(p => Gallery.Press(p, "Centre"))
                .Sound("puzzle_click")
                .ThenGoesTo(p => p.HasFlag("SolvedPortraitGalleryPuzzle") ? "PortraitGallery" : "")

            .Hotspot("Right Medallion", 1039, 454, 252, 252)
                .Describe(p => "The same woman under the same hat, old. Silver hair, deep lines - and exactly the same proud chin." +
                               (p.HasFlag("MetMirelle") ? " Still, unmistakably, Mirélle." : ""))
                .OnAct(p => Gallery.Press(p, "Right"))
                .Sound("puzzle_click")
                .ThenGoesTo(p => p.HasFlag("SolvedPortraitGalleryPuzzle") ? "PortraitGallery" : "")

            .ReturnTo("PortraitGallery", "You step back from the frame.");

        // =========================================================
        // PUZZLE HELPERS (used by the scenes above)
        // =========================================================

        // The Portrait Gallery (see PortraitGallery above).
        private static class Gallery
        {
            public const int WitsNeeded = 12;

            // The portraits were painted 1798, 1821, 1854 - hung centre, left, right.
            private static readonly string[] Order = { "Centre", "Left", "Right" };

            public static int MementoCount(Player p) =>
                p.Inventory.Count(name => Items.GetItem(name).IsMemento);

            public static string MirrorLook(Player p)
            {
                int mementos = MementoCount(p);
                if (mementos >= 2)
                {
                    return "An enormous silver-backed mirror. The face is familiar. The life behind it is not.";
                }
                if (mementos == 1)
                {
                    return "An enormous silver-backed mirror. For a moment, you expect not to recognise the reflection. " +
                           "You do. Somehow that is worse.";
                }
                return "An enormous silver-backed mirror. Your reflection looks back at you. Nothing unusual.";
            }

            public static string Studies(Player p) =>
                "Eyes. Hands. Flowers. Fragments of buildings. Whoever assembled this collection had a generous definition of portrait." +
                (p.HasFlag("MetVesper") ? " The painted fingers suddenly seem much less exaggerated." : "");

            // ACT on the empty frame: Wits finds the mechanism at once; looking
            // around (the frame, the three ages, the sketch) finds it too.
            public static string TouchFrame(Player p)
            {
                p.SetFlag("ExaminedEmptyFrame");

                if (p.HasFlag("DiscoveredPortraitFrameMechanism"))
                {
                    return "Three small brass portrait medallions are set into the lower edge of the frame. They press in, like buttons. A closer look would help.";
                }

                if (p.Meets("Wits", WitsNeeded))
                {
                    p.SetFlag("DiscoveredPortraitFrameMechanism");
                    return $"[Wits {WitsNeeded}] One corner sits slightly proud of the wall. " +
                           "Three small brass portrait medallions are set into the lower edge of the frame.";
                }

                if (p.HasFlag("ExaminedThreeAges") && p.HasFlag("ExaminedGallerySketch"))
                {
                    p.SetFlag("DiscoveredPortraitFrameMechanism");
                    return "Returning to the empty frame, you notice that its three lower ornaments match the frames around the age portraits.";
                }

                return "You run a finger along the inner edge. The frame is considerably deeper than it needs to be.";
            }

            private static int Step(Player p) =>
                p.HasFlag("PortraitSequenceStep2") ? 2 : p.HasFlag("PortraitSequenceStep1") ? 1 : 0;

            private static void ResetSequence(Player p)
            {
                p.ClearFlag("PortraitSequenceStep1");
                p.ClearFlag("PortraitSequenceStep2");
            }

            // The three medallions, pressed in the order the portraits were painted.
            public static string Press(Player p, string medallion)
            {
                int step = Step(p);

                if (Order[step] != medallion)
                {
                    ResetSequence(p);
                    int wrong = p.HasFlag("PortraitWrong2") ? 3 : p.HasFlag("PortraitWrong1") ? 2 : 1;
                    if (wrong <= 2)
                    {
                        p.SetFlag($"PortraitWrong{wrong}");    // counts up to three
                    }

                    string text = "Click. ... The three controls return to their original positions.";
                    if (wrong >= 3)
                    {
                        text += " The portraits are not three members of a family. They are one life, viewed at different times.";
                    }
                    return text;
                }

                if (step == 2)
                {
                    ResetSequence(p);
                    p.SetFlag("SolvedPortraitGalleryPuzzle");
                    return "Click. ... Something releases inside the wall. The empty frame swings slowly open.";
                }

                p.SetFlag($"PortraitSequenceStep{step + 1}");
                return step == 0 ? "Click." : "Click. The second control remains depressed.";
            }

            // Met her, looked at her three portraits, read her pages in the ledger.
            public static void CheckMirelleUnderstanding(Player p)
            {
                if (p.HasFlag("MetMirelle") && p.HasFlag("ExaminedThreeAges") && p.HasFlag("ReadMirelleLedgerEntries"))
                {
                    p.SetFlag("UnderstandsMirellePortraits");
                }
            }
        }

        // The rose bushes: the thorns hurt once, then you know better.
        private static class Roses
        {
            public static string Touch(Player p)
            {
                if (p.HasFlag("PrickedByRoseBush"))
                {
                    return "You eye the thorns. You've already conducted this experiment.";
                }

                if (p.Health <= 2)
                {
                    return "You reach toward the thorns. Something resembling common sense finally intervenes. Perhaps not.";
                }

                p.Health -= 2;
                p.SetFlag("PrickedByRoseBush");
                return "You reach for one of the roses. The thorn answers first. Ow. (-2 Health)";
            }

            public static void Cut(Player p)
            {
                p.Give("Red Rose");
                p.SetFlag("GotRedRose");
            }
        }

        // The gravestone inscription - the clue to the cross.
        private static class Gravestone
        {
            // Wits needed to read it by touch (otherwise: a charcoal rubbing).
            public const int WitsNeeded = 12;

            public const string Clue = "THRICE FOR HEAVEN · ONCE FOR THE HEART · NEVER FOR THE HAND THAT LET GO";

            public const string RubbingText =
                "You hold the paper against the stone and gently rub the charcoal across it. " +
                "Letters invisible to the eye emerge from the worn surface: " + Clue;

            public const string KnownText = "You already know what it says: " + Clue;

            public static string Read(Player p)
            {
                if (p.HasFlag("ReadGravestone"))
                {
                    return KnownText;
                }

                if (p.Meets("Wits", WitsNeeded))
                {
                    p.SetFlag("ReadGravestone");
                    return $"[Wits {WitsNeeded}] You trace the surviving marks with your fingers. Enough remains to reconstruct the pattern: " + Clue;
                }

                return $"[Wits {WitsNeeded} needed - yours is {p.Wits}] You stare at the weathered markings until they start looking like letters simply because you want them to. " +
                       "There must be another way to recover the inscription.";
            }
        }

        // The three spirals on the great cross. Each turns a quarter at a
        // time (stored as story flags like "SpiralUpper2"); the plinth opens
        // when Upper = 3, Left = 1 and Right = 0.
        // Luck needed to catch the mirror with the bucket (otherwise: string).
        private const int WellLuck = 11;

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
