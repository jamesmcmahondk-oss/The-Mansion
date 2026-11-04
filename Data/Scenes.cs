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

            .Hotspot("Fountain", 615, 545, 270, 205)
                .Describe("An ancient stone fountain stands in the courtyard beyond the gates.")
                .GoesTo("Courtyard")

            .Hotspot("Front Door", 905, 505, 95, 140)
                .Describe("The enormous wooden door looks like it has not opened in years.")
                .GoesTo("FrontDoor")

            .Hotspot("Gargoyle", 135, 205, 205, 165)
                .Describe("A sulky and rather sad looking gargoyle sits here.")
                .CloseUp("FrontGargoyle");


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
                .CloseUp("FountainCloseup")

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
                .CloseUp("GlasshouseCloseup")

            .Hotspot("Mansion", 1120, 192, 416, 300)
                .Describe("Ivy smothers the mansion's left wing. Warm light glows in the tall windows.")

            // Listed after the glasshouse so it wins where they overlap.
            .Hotspot("Statue", 770, 330, 120, 270)
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

            // THE FACELESS KNIGHT: the sword only comes free for someone who
            // isn't holding a weapon of their own (put it away by clicking
            // WEAPON in the stats panel).
            .Hotspot("Stone Sword", 825, 350, 55, 425)
                .Describe("A great stone sword, its point resting on the plinth. The hilt is worn smooth, as if many hands have gripped it. " +
                          "Words are cut along the blade: HE WHO WOULD TAKE THE BLADE MUST FIRST SURRENDER HIS OWN.")
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

            .Hotspot("Empty Hands", 825, 350, 55, 425)
                .Describe("The statue's stone hands are folded around nothing now. Under the hood, the darkness seems... satisfied.")
                .OnlyWhen(p => p.HasFlag("TookStoneSword"))

            .ReturnTo("Garden", "You step back from the statue.");


        // The glasshouse on the left of the mansion (from the Garden).
        public static Scene GlasshouseCloseup { get; } =
            NewScene("GlasshouseCloseup",
                     "Assets/Backgrounds/Mansion/glasshouse_closeup.png",
                     "The glasshouse rises before you, all iron lace and misted glass. Inside, hundreds of candles burn among tall palms.")

            .Hotspot("Glass Dome", 200, 192, 1280, 190)
                .Describe("Rain beads on the great glass dome. Ivy has crept almost to the top.")

            .Hotspot("Windows", 50, 400, 350, 300)
                .Describe("Through the misted panes you can make out palms, ferns - and candles, far too many candles.")

            .Hotspot("Windows", 1200, 400, 320, 300)
                .Describe("Through the misted panes you can make out palms, ferns - and candles, far too many candles.")

            .Hotspot("Lantern", 605, 530, 70, 160)
                .Describe("An iron lantern on a stone pillar, lighting the steps.")

            .Hotspot("Lantern", 955, 530, 70, 160)
                .Describe("An iron lantern on a stone pillar, lighting the steps.")

            .Hotspot("Chandelier", 735, 385, 160, 115)
                .Describe("A chandelier blazes just inside, above the doors. Someone lit every candle.")

            .Hotspot("Glasshouse Doors", 720, 500, 180, 290)
                .Describe("Tall glass doors with brass handles. The air seeping out is warm and smells of earth and wax.")
                .OnAct("You push the doors. They don't open - yet they rattle, as if something inside pushed back.")

            .ReturnTo("Garden", "You walk back to the garden.");


        // =========================================================
        // RIGHT OF THE MANSION: side path -> old well -> graveyard
        // Backgrounds made by Tools/build_wide_backgrounds.py.
        // =========================================================

        public static Scene MansionRight { get; } =
            NewScene("MansionRight",
                     "Assets/Backgrounds/Mansion/mansion_right.png",
                     "A wet stone path curves along the mansion's right wing, past a servants' entrance, to an archway in the garden wall. Beyond it, steps climb to an old well.")

            .Hotspot("Lantern", 50, 290, 150, 190)
                .Describe("An iron lantern on a stone post. Its flame burns steadily, despite the rain.")

            .Hotspot("Windows", 240, 192, 400, 310)
                .Describe("Candles burn behind the tall windows, but nothing moves inside.")

            .Hotspot("Servants' Entrance", 790, 300, 180, 370)
                .Describe("A smaller door at the top of a few worn steps - the servants' entrance. Lanterns burn on either side.")
                .OnAct("You knock on the door, but there is no response. Jiggling the handle reveals a surprise! ...It's locked!")
                // Later: add a second "Servants' Entrance" hotspot with
                // .GoesTo("...") and .OnlyWhen(p => p.HasFlag("ServantsDoorOpen")),
                // and .OnlyWhen(p => !p.HasFlag("ServantsDoorOpen")) on this one.

            .Hotspot("Garden Arch", 1140, 400, 160, 380)
                .Describe("An ivy-covered archway in the garden wall. Steps beyond it climb towards the well.")
                .GoesTo("Well")

            // Listed after the arch so it wins where the two overlap.
            .Hotspot("Path to the Back", 1060, 555, 125, 175)
                .Describe("The cobbled path curves away along the garden wall, around to the back of the mansion.")
                .GoesTo("RearYard")

            .Hotspot("Old Well", 1160, 250, 130, 120)
                .Describe("A small roofed well stands at the top of the hill.")
                .GoesTo("Well")

            .Hotspot("Rose Bush", 1300, 500, 236, 190)
                .Describe("A great tangle of blood-red roses, impossibly in bloom this late in the year.")
                .OnlyWhen(p => !p.HasFlag("GotRedRose"))
                .OnAct(p => Roses.Touch(p))
                .OnItem("Garden Shears", "Snip. Much better. You cut one perfect dark-red rose.",
                        effect: p => Roses.Cut(p),
                        onlyIf: p => !p.HasFlag("GotRedRose"))

            .Hotspot("Rose Bush", 1300, 500, 236, 190)
                .Describe("A great tangle of blood-red roses. There's a small gap where you cut one.")
                .OnlyWhen(p => p.HasFlag("GotRedRose"))
                .OnAct(p => Roses.Touch(p))
                .OnItem("Garden Shears", "Snip. Much better. You cut one perfect dark-red rose.",
                        effect: p => Roses.Cut(p),
                        onlyIf: p => !p.HasFlag("GotRedRose"))
                .OnItem("Garden Shears", "You already have a rose. Leave the rest of them be.")

            // The bench: sit on it (ACT) first. Then you hear something
            // underneath, and LOOK opens the close-up with Ebria.
            .Hotspot("Stone Bench", 1250, 690, 220, 142)
                .Describe("A mossy stone bench beside the roses.")
                .OnAct("You sit down on the cold, damp stone... and from somewhere underneath you comes a faint hiccup. Then a tiny, slurred giggle.",
                       p => p.SetFlag("HeardBenchNoise"))
                .OnlyWhen(p => !p.HasFlag("HeardBenchNoise"))

            .Hotspot("Stone Bench", 1250, 690, 220, 142)
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
                .Describe("Narrow service windows overlook the yard. A few glow warmly despite the abandoned courtyard below.")

            .Hotspot("Windows", 410, 283, 45, 96)
                .Describe("Narrow service windows overlook the yard. A few glow warmly despite the abandoned courtyard below.")

            .Hotspot("Lantern", 560, 208, 55, 107)
                .Describe("A lantern burns beside the service door. Someone keeps these lit.")

            // Bolted from inside - never opened by a key. Later: drawing the
            // bolt from the kitchen sets KitchenDoorUnlocked and this becomes
            // a shortcut (.GoesTo("Kitchen") with .OnlyWhen).
            .Hotspot("Kitchen Door", 603, 245, 192, 363)
                .Describe("A heavy service door leads into the Mansion, with a small barred window and heavy iron hardware. " +
                          "Generations of boots have worn the edges of the stone steps smooth.")
                .OnAct("The door shifts half an inch, then stops against something solid. Bolted from the other side.")

            // Something underneath holds them shut. The knock happens once.
            .Hotspot("Cellar Doors", 256, 544, 300, 128)
                .Describe("A pair of heavy cellar doors descend beneath the Mansion. The left door doesn't quite sit flush with its frame.")
                .OnAct("You pull the iron ring. The door rises an inch. Stops. Something underneath is holding it shut. ... " +
                       "A single quiet sound comes from underneath. Knock.",
                       p => p.SetFlag("TriedCellarDoor"))
                .OnlyWhen(p => !p.HasFlag("TriedCellarDoor"))

            .Hotspot("Cellar Doors", 256, 544, 300, 128)
                .Describe("A pair of heavy cellar doors descend beneath the Mansion. The left door doesn't quite sit flush with its frame.")
                .OnAct("Still blocked. Whatever is beneath it isn't moving.")
                .OnlyWhen(p => p.HasFlag("TriedCellarDoor"))

            .Hotspot("Clothesline", 810, 288, 240, 22)
                .Describe("An old washing line stretches across the yard. Almost everything that once hung from it is gone.")

            // It is simply The Sock.
            .Hotspot("Sock", 933, 322, 30, 48)
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
                .Describe("Rainwater disappears through an iron grate beneath the Mansion. " +
                          "A few tiny pale fragments have collected against the grate. Probably kitchen scraps.")
                .OnAct("The grate is firmly set into the cobbles.")

            // ---- The tool shed --------------------------------------
            // (No close-up picture yet: the door opens onto the workbench,
            // and the useful things are taken one at a time.)
            .Hotspot("Tool Shed", 990, 192, 480, 205)
                .Describe("A small gardening shed leans against the wall, its roof thick with moss. " +
                          "The tools outside have been left exactly where someone last used them. Apparently nobody came back for them.")

            .Hotspot("Shed Door", 1061, 400, 118, 293)
                .Describe("A small gardening shed leans against the wall, its roof thick with moss.")
                .OnAct("The shed door scrapes open. Inside: shelves of rusted tins, a little workbench, a pair of garden shears and a few sticks of charcoal.",
                       p => p.SetFlag("OpenedToolShed"))
                .OnlyWhen(p => !p.HasFlag("OpenedToolShed"))

            .Hotspot("Workbench", 1061, 400, 118, 293)
                .Describe("A sturdy pair of pruning shears lies on the workbench - rusty, but still serviceable. Beside them, a few sticks of charcoal.")
                .OnAct("You take the garden shears.",
                       p =>
                       {
                           p.Give("Garden Shears");
                           p.SetFlag("GotGardenShears");
                       })
                .OnlyWhen(p => p.HasFlag("OpenedToolShed") && !p.HasFlag("GotGardenShears"))

            .Hotspot("Workbench", 1061, 400, 118, 293)
                .Describe("A few sticks of charcoal lie on the workbench. Messy, but still usable.")
                .OnAct("You take the charcoal, and most of its dust with it.",
                       p =>
                       {
                           p.Give("Charcoal");
                           p.SetFlag("GotCharcoal");
                       })
                .OnlyWhen(p => p.HasFlag("GotGardenShears") && !p.HasFlag("GotCharcoal"))

            .Hotspot("Workbench", 1061, 400, 118, 293)
                .Describe("Rusted tins, broken pots and a locked cabinet. Nothing else worth taking.")
                .OnAct("There's nothing else in the shed worth carrying around.")
                .OnlyWhen(p => p.HasFlag("GotGardenShears") && p.HasFlag("GotCharcoal"))

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
                .Describe("An old pitchfork. Several tines have rusted away.")

            .Hotspot("Rake", 1292, 411, 75, 229)
                .Describe("A rake with half its teeth missing.")

            .ReturnTo("MansionRight", "You head back along the side of the Mansion.");


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
                .Describe("Candles burn in the Mansion's windows. Someone keeps a lot of candles.")

            .Hotspot("Windows", 160, 192, 65, 270)
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
            .Sprite("Assets/Characters/Ebria_UnderBench.png", 429, 421, 397)

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
                .Describe("Cold air rises from the well. You lean over to look inside...")
                .CloseUp("WellCloseup")
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
                .CloseUp("GravestoneCloseup")

            .Hotspot("Stone Cross", 1000, 260, 140, 250)
                .Describe("A tall Celtic cross stands at the top of the steps, watching over the graves.")
                .CloseUp("CrossCloseup")

            .Hotspot("Graveyard Path", 870, 560, 220, 240)
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

                    if (p.TestLuck())
                    {
                        p.Give("Cracked Hand Mirror");
                        p.SetFlag("GotWellMirror");
                        return "You lower the bucket and scrape it along the bottom... When it comes back up, a small silver hand mirror is lying in it. The glass is cracked straight across.";
                    }

                    return "The bucket thuds against the bottom and swings straight past the glint. Maybe try again?";
                })

            .Hotspot("Something Glinting", 772, 702, 66, 54)
                .Describe("At the very bottom of the well, something small and silver catches the light.")
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

            .Hotspot("Moon", 1040, 192, 110, 110)
                .Describe("The full moon hangs over the graveyard. It looks closer than it should.")

            .Hotspot("Celtic Cross", 600, 192, 430, 420)
                .Describe("Knots weave in and out of each other without beginning or end. At the centre, three spirals chase one another: " +
                          "one reaching up, one to the left, one to the right.")
                .OnTalk("You read the knots aloud, as if they were words. They are not. You feel slightly foolish.")

            // THE GRAVEYARD SECRET: once the gravestone clue is known, the
            // three spirals turn (ACT). Thrice for heaven (up), once for the
            // heart (left), never for the hand that let go (right).
            .Hotspot("Upper Spiral", 782, 322, 32, 30)
                .Describe("A small stone spiral at the very centre of the cross, reaching upward.")
                .OnAct(p => CrossSpirals.Turn(p, "Upper"))

            .Hotspot("Left Spiral", 762, 350, 32, 30)
                .Describe("A small stone spiral at the centre of the cross, curling to the left.")
                .OnAct(p => CrossSpirals.Turn(p, "Left"))

            .Hotspot("Right Spiral", 798, 350, 32, 30)
                .Describe("A small stone spiral at the centre of the cross, curling to the right.")
                .OnAct(p => CrossSpirals.Turn(p, "Right"))

            .Hotspot("Plinth", 530, 612, 520, 200)
                .Describe("A mossy stepped plinth. A small knotwork panel is set into its face, as if something might be hidden behind it.")
                .OnAct("You press the carved panel. It doesn't budge - but the stone around it sounds hollow.")
                .OnlyWhen(p => !p.HasFlag("CrossOpened"))

            .Hotspot("Open Plinth", 530, 612, 520, 200)
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

            .Hotspot("Empty Plinth", 530, 612, 520, 200)
                .Describe("The hollow in the plinth is empty now.")
                .OnlyWhen(p => p.HasFlag("FoundPhotograph"))

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

            // Stats create shortcuts; items provide reliable solutions:
            // ACT is a Wits test (retry allowed), Charcoal + Paper always works.
            .Hotspot("Inscription", 680, 480, 280, 240)
                .Describe("Most of the inscription has been worn smooth. A few shallow marks remain, but not enough to read.")
                .OnAct(p => Gravestone.Read(p))
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
        // PUZZLE HELPERS (used by the scenes above)
        // =========================================================

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

                if (p.TestWits())
                {
                    p.SetFlag("ReadGravestone");
                    return "You trace the surviving marks with your fingers. Enough remains to reconstruct the pattern: " + Clue;
                }

                return "You stare at the weathered markings until they start looking like letters simply because you want them to. " +
                       "There must be another way to recover the inscription.";
            }
        }

        // The three spirals on the great cross. Each turns a quarter at a
        // time (stored as story flags like "SpiralUpper2"); the plinth opens
        // when Upper = 3, Left = 1 and Right = 0.
        private static class CrossSpirals
        {
            private static readonly string[] Directions = { "up", "right", "down", "left" };

            private static int Get(Player p, string spiral)
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

            private static void Set(Player p, string spiral, int turns)
            {
                for (int t = 1; t < 4; t++)
                {
                    p.ClearFlag($"Spiral{spiral}{t}");
                }
                if (turns > 0)
                {
                    p.SetFlag($"Spiral{spiral}{turns}");
                }
            }

            public static string Turn(Player p, string spiral)
            {
                if (p.HasFlag("CrossOpened"))
                {
                    return "The spirals won't move any more.";
                }

                if (!p.HasFlag("ReadGravestone"))
                {
                    return "You push the carving. Nothing.";
                }

                int turns = (Get(p, spiral) + 1) % 4;
                Set(p, spiral, turns);

                // Each spiral starts pointing its own way; report where its tail now points.
                int start = spiral switch { "Upper" => 0, "Right" => 1, _ => 3 };
                string direction = Directions[(start + turns) % 4];
                string text = $"The {spiral.ToLowerInvariant()} spiral grinds a quarter turn beneath your fingers. Its tail now points {direction}.";

                if (Get(p, "Upper") == 3 && Get(p, "Left") == 1 && Get(p, "Right") == 0)
                {
                    p.SetFlag("CrossOpened");
                    text += " Somewhere below, stone slides against stone. The panel in the plinth has opened.";
                }

                return text;
            }
        }


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
